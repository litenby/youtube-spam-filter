using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace youtube_spam_filter
{
    internal class SpamFilter

    {
        private YouTubeService ytService;
        private UserCredential credential;
        public Action<string> Log { get; set; }

        Timer Timer1;
        Timer Timer2;
        Timer Timer3;

        int firstConnect = 0;
        int stage = 0;
        int done = 1;
        int messagesDeletedCounter = 0;
        int startUpMsgHoldBack = 1;
        int recentMsgIndex = 0;
        int warmupDelay = 1;

        String nextpagetoken = "";
        String msgToSend = "";
        String msgToDelete = "";
        String[] recentMessages = new string[75];
        String currentAnswer = "";

        DateTime askTime;

        List<string> spamKeywords = new List<string>();

        public AppConfig config;

        private string ClientSecretsFile => Path.Combine(config.BasePath, "client_secrets.json");
        private string SpamKeywordsFile => Path.Combine(config.BasePath, "spam_keywords.txt");

        static void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit] = newText;
            File.WriteAllLines(fileName, arrLine);
        }
        private void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            if (!File.Exists(configPath))
            {
                Log?.Invoke("Config file not found:\n" + configPath + "Configuration Error");
                return;
            }

            string json = File.ReadAllText(configPath);
            config = JsonConvert.DeserializeObject<AppConfig>(json);
            Log?.Invoke("Loaded configuration file. LiveChatId status: " + (string.IsNullOrEmpty(config.LiveChatId) ? "[none]" : "[loaded]"));
            Log?.Invoke("LiveChatId: " + config.LiveChatId);
        }
        void LoadSpamKeywords()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "spam_keywords.txt");
            Log?.Invoke(filePath);
            if (File.Exists(filePath))
            {
                spamKeywords = File.ReadAllLines(filePath)
                                  .Select(line => line.Trim().ToLower())
                                  .Where(line => !string.IsNullOrWhiteSpace(line))
                                  .ToList();
                Log?.Invoke($"Loaded {spamKeywords.Count} spam keywords.");
            }
            else
            {
                Log?.Invoke("Warning: spam_keywords.txt not found. No spam filtering will occur.");
            }
        }

        public bool checkSpam(string msgToCheck)
        {
            string allLower = msgToCheck.ToLower();
            return spamKeywords.Any(keyword => allLower.Contains(keyword));
        }

        void Timer1_Tick(object state)
        {
            try
            {
                getMsg(currentAnswer);
                Log?.Invoke(DateTime.Now + " getting new Youtube livechat messages");
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Log?.Invoke("Error: " + e.Message);
                }
            }
            GC.Collect();
            Thread.Sleep(500);
        }

        void Timer3_Tick(object state)
        {
            try
            {
                warmupDelay = 0;
                Log?.Invoke("Messages now allowed to be sent to Youtube chat.");
                Timer3.Change(Timeout.Infinite, Timeout.Infinite);
                startUpMsgHoldBack = 0;
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Log?.Invoke("Error: " + e.Message);
                }
            }
            GC.Collect();
            Thread.Sleep(500);
        }
        public void Disconnect()
        {
            Log?.Invoke("Disconnecting from YouTube and stopping all timers.");

            Timer1?.Change(Timeout.Infinite, Timeout.Infinite);
            Timer2?.Change(Timeout.Infinite, Timeout.Infinite);
            Timer3?.Change(Timeout.Infinite, Timeout.Infinite);
            
            Timer1?.Dispose();
            Timer2?.Dispose();
            Timer3?.Dispose();
           
            ytService = null;
            credential = null;

            Log?.Invoke("Spam filter has been disconnected.");
        }

        public async void Start()
        {
            Thread.Sleep(1000);
            Log?.Invoke("Loading configuration file.");
            LoadConfig();
            Thread.Sleep(1000);

            using (var stream = new FileStream(ClientSecretsFile, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            ytService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()

            });
            Log?.Invoke("Loading spam keywords.");
            LoadSpamKeywords();
            Log?.Invoke("Starting spam filter timers.");
            Timer1 = new System.Threading.Timer(Timer1_Tick, null, 3000, 2000);        // Retrieves channel messages every 2 seconds.
            Timer3 = new System.Threading.Timer(Timer3_Tick, null, 15000, 15000);      // Asks trivia question, waits 15 seconds, gives hint, then gives answer.
        }

        public async Task sendMsg(string myMessage)
        {
            if (warmupDelay == 0)
            {
                Log?.Invoke("Sent Message: " + myMessage);

                firstConnect = 1;
                LiveChatMessage comments = new LiveChatMessage();
                LiveChatMessageSnippet mySnippet = new LiveChatMessageSnippet();
                LiveChatTextMessageDetails txtDetails = new LiveChatTextMessageDetails();
                txtDetails.MessageText = myMessage;
                mySnippet.TextMessageDetails = txtDetails;
                mySnippet.LiveChatId = config.LiveChatId;
                mySnippet.Type = "textMessageEvent";
                comments.Snippet = mySnippet;
                comments = await ytService.LiveChatMessages.Insert(comments, "snippet").ExecuteAsync();
            }
            else
            {
                Log?.Invoke("Message not sent until program startup complete: " + myMessage);
            }
        }

        public async Task getMsg(String curAnswer)
        {
            Console.WriteLine("Getting messages.");

            String liveChatId = config.LiveChatId;
            var chatMessages = ytService.LiveChatMessages.List(liveChatId, "id,snippet,authorDetails");
            chatMessages.PageToken = nextpagetoken;

            var chatResponse = await chatMessages.ExecuteAsync();
            nextpagetoken = chatResponse.NextPageToken;

            Log?.Invoke($"nextpagetoken is {nextpagetoken}");
            Log?.Invoke($"Received {chatResponse.Items.Count} messages");

            foreach (var chatMessage in chatResponse.Items)
            {
                string messageId = chatMessage.Id;
                string displayName = chatMessage.AuthorDetails.DisplayName;
                string displayMessage = chatMessage.Snippet.DisplayMessage;
                DateTime messageTime = chatMessage.Snippet.PublishedAt ?? DateTime.UtcNow;
                TimeSpan timeSince = DateTime.UtcNow - messageTime;
                int toSeconds = (int)timeSince.TotalSeconds;

                Log?.Invoke($"{DateTime.Now} msg time: {messageTime}  ago: {timeSince} " + displayMessage);
                

                if (displayName != config.DisplayName && !recentMessages.Contains(messageId) && startUpMsgHoldBack == 0)
                {
                    //Log?.Invoke($"recent message: {messageTime} Delay: {toSeconds}  {displayMessage}");

                    // Track seen messages
                    recentMessages[recentMsgIndex] = messageId;
                    recentMsgIndex = (recentMsgIndex + 1) % recentMessages.Length;

                    if (checkSpam(displayMessage))
                        Log?.Invoke("Found spam, deleting message: " + displayMessage);
                        await deleteMsg(messageId, displayMessage);
                }
            }
        }

        public async Task deleteMsg(String mGetRidOf, String mBody)
        {
            messagesDeletedCounter++;
            
            await ytService.LiveChatMessages.Delete(mGetRidOf).ExecuteAsync();
            Log?.Invoke($"{DateTime.Now}  '{mBody}' deleted from channel. {messagesDeletedCounter} spam messages deleted.");
        }
    }
}
