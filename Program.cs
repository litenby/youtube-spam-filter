using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpamBotNamespace
{
    internal class SpamBot
    {
        Timer Timer1;
        Timer Timer2;
        Timer Timer3;
        int firstConnect = 0;
        String nextpagetoken1;
        String nextpagetoken2;
        String nextpagetoken3;
        String nextpagetoken4;
        String nextpagetoken5;
        int stage = 0;
        int done = 1;
        int currentQuestionLine = 0;
        String[] recentMessages = new string[75];
        DateTime askTime;
        String msgToSend = "";
        int startUpMsgHoldBack = 1; 
        String msgToDelete = "";
        int messagesDeletedCounter = 0;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Spam Bot");
            Console.WriteLine("==========");
            SpamBot newBot = new SpamBot();
            newBot.start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        void stackMessages(String input1)
        {

            for (int i = 0; i < 74; i++)
            {
                recentMessages[i + 1] = recentMessages[i];
            }
            recentMessages[0] = input1;
        }

        void addQuestion(String newQuestion)
        {
            int counter = 0;
            string myLine = "";
            string question = "";
            string answer = "";
            System.IO.StreamReader file =
            new System.IO.StreamReader("c:\\spambot\\questions.txt");

            while ((myLine = file.ReadLine()) != null)
            {
                counter++;
            }
            string[] values = newQuestion.Split('#');
            question = values[0];
            answer = values[1];
            question = question.Remove(0, 5);
            file.Close();
            File.AppendAllText("c:\\spambot\\answers.txt", answer + Environment.NewLine);
            File.AppendAllText("c:\\spambot\\questions.txt", question + Environment.NewLine);
        }

        static void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit] = newText;
            File.WriteAllLines(fileName, arrLine);
        }

        public Boolean CheckSpam(String msgToCheck)
        {
            Boolean result = false;
            String allLower = msgToCheck.ToLower();

            if (allLower.Contains("vark")) result = true;
            if (allLower.Contains("vasf")) result = true;
            if (allLower.Contains("vawr")) result = true;
            if (allLower.Contains("vog")) result = true;
            if (allLower.Contains("voi")) result = true;
            if (allLower.Contains("vot")) result = true;
            if (allLower.Contains("tech")) result = true;
            if (allLower.Contains("fyi")) result = true;
            if (allLower.Contains("your-dreams")) result = true;
            if (allLower.Contains("(.)")) result = true;
            if (allLower.Contains("vor.")) result = true;
            if (allLower.Contains(". ong")) result = true;
            if (allLower.Contains(".site")) result = true;
            if (allLower.Contains(".rent")) result = true;
            if (allLower.Contains(".ngo")) result = true;
            if (allLower.Contains(".ong")) result = true;
            if (allLower.Contains(". ngo")) result = true;
            if (allLower.Contains(". rent")) result = true;
            if (allLower.Contains(". site")) result = true;
            if (allLower.Contains(". red")) result = true;
            if (allLower.Contains(".red")) result = true;
            if (allLower.Contains(".online")) result = true;
            if (allLower.Contains(". online")) result = true;
            
            return result;
        }

        void Timer1_Tick(object state)
        {
            try
            {
                getMsg(currentAnswer);
               // Console.WriteLine(DateTime.Now + " getting new messages");
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            GC.Collect();
            Thread.Sleep(500);
        }
          
        void Timer3_Tick(object state)
        {
            try
            {
                startUpMsgHoldBack = 0;
                Console.WriteLine("Messages now allowed to be sent.");
                Timer3.Change(Timeout.Infinite, Timeout.Infinite);

            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            GC.Collect();
            Thread.Sleep(500);
        }

        private void start()
        {
            Console.WriteLine("start function");
            Timer1 = new Timer(Timer1_Tick, null, 34000, 300000);  // Delay for retrieving channel chat messages. Changed from 2000 to 60000
            Timer3 = new Timer(Timer3_Tick, null, 15000, 15000);
            Timer4 = new Timer(Timer4_Tick, null, 30000, 3600000);
            Console.WriteLine("start function done");
        }

        public async Task sendMsg(string myMessage)
        {
            Console.WriteLine("sendmsg function");
            if (startUpMsgHoldBack == 0)
            {
                Console.WriteLine("SENT " + myMessage);
                UserCredential credential;

                using (var stream = new FileStream("c:\\spambot\\client_secrets.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        // This OAuth 2.0 access scope allows for full read/write access to the
                        // authenticated user's account.
                        new[] { YouTubeService.Scope.Youtube },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(this.GetType().ToString())
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
                mySnippet.LiveChatId = "x";

                mySnippet.Type = "textMessageEvent";
                comments.Snippet = mySnippet;
                comments = await youtubeService.LiveChatMessages.Insert(comments, "snippet").ExecuteAsync();
            }
            else
            {
                Console.WriteLine("HELD BACK " + myMessage);
            }
        }

        public async Task getMsg(String curAnswer)
        {
            Console.WriteLine("Getting channel 3 messages.");
            UserCredential credential;

            using (var stream = new FileStream("c:\\spambot\\client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            var ytService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            String liveChatId = "x";
            var chatMessages = ytService.LiveChatMessages.List(liveChatId, "id,snippet,authorDetails");
            chatMessages.PageToken = nextpagetoken3;
            var chatResponse = await chatMessages.ExecuteAsync();
            nextpagetoken3 = chatResponse.NextPageToken;
            //Console.WriteLine("nextpagetoken is " + nextpagetoken);
            long? pollinginterval = chatResponse.PollingIntervalMillis;
            PageInfo pageInfo = chatResponse.PageInfo;
            List<LiveChatMessageListResponse> messages = new List<LiveChatMessageListResponse>();
            //Console.WriteLine(chatResponse.PageInfo.TotalResults + " total messages " + chatResponse.PageInfo.ResultsPerPage + " results per page" + nextpagetoken);

            foreach (var chatMessage in chatResponse.Items)
            {
                string messageId = chatMessage.Id;
                string displayName = chatMessage.AuthorDetails.DisplayName;
                string displayMessage = chatMessage.Snippet.DisplayMessage;
                System.DateTime messageTime = chatMessage.Snippet.PublishedAt.Value;
             
                var now = DateTime.Now;
                var timeSince = now - messageTime;
                int toSeconds = timeSince.Seconds;
                // Console.WriteLine(DateTime.Now + "   msg time: " + messageTime + "  ago: " + timeSince);

                // && toSeconds < 33 && toSeconds > 25 
                if (displayName != "Trivia Bot" && recentMessages.Contains(messageId).Equals(false) && startUpMsgHoldBack == 0)
                {
                    // stackMessages(messageId);
                    Console.WriteLine("recent message: " + messageTime + " Delay: " + toSeconds + "  " + displayMessage);

                    if (displayMessage.Contains("!add"))
                    {
                        addQuestion(displayMessage);
                        String msg = "Question added.";
                        sendMsg(msg);
                    }

                    if (checkSpam(displayMessage) == true) await deleteMsg(messageId, displayMessage, 3);
                
                }
            }
        }

        public async Task deleteMsg(String mGetRidOf, String mBody, int channelNum)
        {
            messagesDeletedCounter = messagesDeletedCounter + 1;
            Console.WriteLine(DateTime.Now + "  " + mBody + " deleted from channel " + channelNum + ". " + messagesDeletedCounter + " spam messages deleted.");

            UserCredential credential;

            using (var stream = new FileStream("c:\\spambot\\client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            var ytService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });
           
            var msgDeleter = ytService.LiveChatMessages.Delete(mGetRidOf);
            var doIt = await msgDeleter.ExecuteAsync();

        }
    }
}