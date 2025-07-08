using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using youtube_spam_filter;

namespace SpamFilterNamespace
{
    internal class SpamFilter
    {
        Timer Timer1;
        Timer Timer2;
        Timer Timer3;
        int firstConnect = 0;
        String nextpagetoken = "";
        int stage = 0;
        int done = 1;
        String[] recentMessages = new string[75];
        int recentMsgIndex = 0;
        DateTime askTime;
        String msgToSend = "";
        int startUpMsgHoldBack = 1;
        String msgToDelete = "";
        int messagesDeletedCounter = 0;
        String currentAnswer = "";
        List<string> spamKeywords = new List<string>();
        private AppConfig config;


        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Spam Filter");
            Console.WriteLine("==========");
            SpamFilter newFilter = new SpamFilter();
            newFilter.start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

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
                Console.WriteLine("Config file not found: " + configPath);
                return;
            }

            string json = File.ReadAllText(configPath);
            config = JsonConvert.DeserializeObject<AppConfig>(json);
            Console.WriteLine("Loaded config. LiveChatId: " + (string.IsNullOrEmpty(config.LiveChatId) ? "[none]" : "[loaded]"));
        }
        void LoadSpamKeywords()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "spam_keywords.txt");
            if (File.Exists(filePath))
            {
                spamKeywords = File.ReadAllLines(filePath)
                                  .Select(line => line.Trim().ToLower())
                                  .Where(line => !string.IsNullOrWhiteSpace(line))
                                  .ToList();
                Console.WriteLine($"Loaded {spamKeywords.Count} spam keywords.");
            }
            else
            {
                Console.WriteLine("Warning: spam_keywords.txt not found. No spam filtering will occur.");
            }
        }

        public bool checkSpam(string msgToCheck)
        {
            string allLower = msgToCheck.ToLower();
            return spamKeywords.Any(keyword => allLower.Contains(keyword));
        }

        void Timer1_Tick(object state)
        {
            _ = HandleGetMsgAsync();
        }

        async Task HandleGetMsgAsync()
        {
            try
            {
                Console.WriteLine(DateTime.Now + " getting new messages");
                await getMsg(currentAnswer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception in HandleGetMsgAsync: " + ex.Message);
            }

            await Task.Delay(500);
            GC.Collect();
        }

        void Timer3_Tick(object state)
        {
            _ = HandleStartupAsync();
        }

        async Task HandleStartupAsync()
        {
            try
            {
                startUpMsgHoldBack = 0;
                Console.WriteLine("Messages now allowed to be sent.");
                Timer3.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception in HandleStartupAsync: " + ex.Message);
            }

            await Task.Delay(500);
            GC.Collect();
        }

        private void start()
        {
            Console.WriteLine("Loading spam keywords.");
            LoadSpamKeywords();
            Console.WriteLine("Loading configuration file.");
            LoadConfig();
            Console.WriteLine("Starting timer 1.");
            Timer1 = new Timer(Timer1_Tick, null, 34000, 300000);
            Console.WriteLine("Starting timer 2.");
            Timer3 = new Timer(Timer3_Tick, null, 15000, 15000);
            Console.WriteLine("Startup complete.");
        }

        public async Task sendMsg(string myMessage)
        {
            Console.WriteLine("sendmsg function");
            if (startUpMsgHoldBack == 0)
            {
                Console.WriteLine("Sent message: " + myMessage);
                UserCredential credential;

                
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json");
                //Console.WriteLine("Looking for file at: " + path);
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { YouTubeService.Scope.Youtube },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(@"C:\spambot\tokenstore", true)
                    );
                }

                firstConnect = 1;
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = this.GetType().ToString()
                });

                LiveChatMessage comments = new LiveChatMessage();
                LiveChatMessageSnippet mySnippet = new LiveChatMessageSnippet();
                LiveChatTextMessageDetails txtDetails = new LiveChatTextMessageDetails();
                txtDetails.MessageText = myMessage;
                mySnippet.TextMessageDetails = txtDetails;
                mySnippet.LiveChatId = config.LiveChatId;
                mySnippet.Type = "textMessageEvent";
                comments.Snippet = mySnippet;

                await youtubeService.LiveChatMessages.Insert(comments, "snippet").ExecuteAsync();
            }
            else
            {
                Console.WriteLine("Message delayed until program warm-up complete. " + myMessage);
            }
        }

        public async Task getMsg(String curAnswer)
        {
            Console.WriteLine("Getting messages.");
            UserCredential credential;

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json");
            //Console.WriteLine("Looking for file at: " + path);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(@"C:\spambot\tokenstore", true)
                );
            }

            var ytService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            String liveChatId = config.LiveChatId;
            var chatMessages = ytService.LiveChatMessages.List(liveChatId, "id,snippet,authorDetails");
            chatMessages.PageToken = nextpagetoken;

            var chatResponse = await chatMessages.ExecuteAsync();
            nextpagetoken = chatResponse.NextPageToken;

            Console.WriteLine($"nextpagetoken is {nextpagetoken}");
            Console.WriteLine($"Received {chatResponse.Items.Count} messages");

            foreach (var chatMessage in chatResponse.Items)
            {
                string messageId = chatMessage.Id;
                string displayName = chatMessage.AuthorDetails.DisplayName;
                string displayMessage = chatMessage.Snippet.DisplayMessage;
                DateTime messageTime = chatMessage.Snippet.PublishedAt ?? DateTime.UtcNow;
                TimeSpan timeSince = DateTime.UtcNow - messageTime;
                int toSeconds = (int)timeSince.TotalSeconds;

                Console.WriteLine($"{DateTime.Now}   msg time: {messageTime}  ago: {timeSince}");

                if (displayName != "Trivia Bot" && !recentMessages.Contains(messageId) && startUpMsgHoldBack == 0)
                {
                    Console.WriteLine($"recent message: {messageTime} Delay: {toSeconds}  {displayMessage}");

                    // Track seen messages
                    recentMessages[recentMsgIndex] = messageId;
                    recentMsgIndex = (recentMsgIndex + 1) % recentMessages.Length;

                    if (checkSpam(displayMessage))
                        await deleteMsg(messageId, displayMessage);
                }
            }
        }



        public async Task deleteMsg(String mGetRidOf, String mBody)
        {
            messagesDeletedCounter++;
            Console.WriteLine($"{DateTime.Now}  '{mBody}' deleted from channel. {messagesDeletedCounter} spam messages deleted.");

            UserCredential credential;

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json");
            //Console.WriteLine("Looking for file at: " + path);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(@"C:\spambot\tokenstore", true)
                );
            }

            var ytService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            await ytService.LiveChatMessages.Delete(mGetRidOf).ExecuteAsync();
        }
    }
}
