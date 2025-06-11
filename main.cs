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

namespace TriviaBot20
{

    public class Airport
    {
        public string code { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string name { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string woeid { get; set; }
        public string tz { get; set; }
        public string phone { get; set; }
        public string type { get; set; }
        public string email { get; set; }
        public string url { get; set; }
        public string runway_length { get; set; }
        public string elev { get; set; }
        public string icao { get; set; }
        public string direct_flights { get; set; }
        public string carriers { get; set; }

        public String getAirportInfo(string icao_input)
        {
            string[] parts = icao_input.Split(' '); // remove the !info from the ICAO code

            var jsonText = File.ReadAllText("c:\\triviabot\\airports.json");
            var records = JsonConvert.DeserializeObject<IList<Airport>>(jsonText);

            var s = records.FirstOrDefault(x => x.icao == parts[1]);

            string icao_output = s.icao + " | " + s.name + " | " + s.city + ", " + s.state + " | " + s.country + " | Elev " + s.elev + "ft | Runway " + s.runway_length + " ft | Direct Flights " + s.direct_flights + " | Carriers " + s.carriers + " | Phone " + s.phone + " | Coords " + s.lat + ", " + s.lon;
            return icao_output;
        }
    }

    // For getting the live weather reports about any airport

    public class AirportWx
    {
        public String getAirportWx(string message)
        {
            string[] parts = message.Split(' ');
            
            string WEBSERVICE_URL = "https://avwx.rest/api/metar/" + parts[1];
            try
            {
                var webRequest = System.Net.WebRequest.Create(WEBSERVICE_URL);
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.Timeout = 20000;
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add("Authorization", "x");
                    using (System.IO.Stream s = webRequest.GetResponse().GetResponseStream())
                    {

                        using (System.IO.StreamReader sr = new System.IO.StreamReader(s))
                        {
                            var jsonResponse = sr.ReadToEnd();
                            string json = jsonResponse;
                            JObject o = JObject.Parse(jsonResponse);
                            string Station = o.SelectToken("Station").ToString();
                            string Altimeter = o.SelectToken("Altimeter").ToString();
                            string Dewpoint = o.SelectToken("Dewpoint").ToString();
                            string FlightRules = o.SelectToken("Flight-Rules").ToString();
                            string Temperature = o.SelectToken("Temperature").ToString();
                            string Time = o.SelectToken("Time").ToString();
                            string Visibility = o.SelectToken("Visibility").ToString();
                            string Winddirection = o.SelectToken("Wind-Direction").ToString();
                            string Windgust = o.SelectToken("Wind-Gust").ToString();
                            string Windspeed = o.SelectToken("Wind-Speed").ToString();

                            string currentMetar = Station + " | " +
                               Time + " | Flt Rules: " + FlightRules +
                                           " | Temp:  " + Temperature + "C " +
                                           " | Alt: " + Altimeter + "inHg " +
                                           " | Dew: " + Dewpoint +
                                           " | Vis: " + Visibility + "sm " +
                                           " | Wind: " + Winddirection + " at "
                                           + Windspeed + "kts";
                            return currentMetar;
                        }
                    }
                }
                else
                {
                    string blah = "";
                    return blah;
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine(ex.ToString());
                return ex.ToString();
            }
        }
    }

    internal class TriviaBot
    {
        Timer Timer1;
        Timer Timer2;
        Timer Timer3;
        Timer Timer4;
        Timer Timer5;
        Timer Timer6;
        Timer Timer7;
        Timer Timer8;
        int firstConnect = 0;
        String nextpagetoken1;
        String nextpagetoken2;
        String nextpagetoken3;
        String nextpagetoken4;
        String nextpagetoken5;
        int stage = 0;
        int done = 1;
        String currentQuestion = "0";
        String currentAnswer = "0";
        int currentQuestionLine = 0;
        String[] recentMessages = new string[75];
        int questionAnswered = 0;
        DateTime askTime;
        String hint1 = "";
        String msgToSend = "";
        int startUpMsgHoldBack = 1; // Prevents messages from being sent to channel during first few seconds of program. Avoid flood of messages from reading pages. 
        String msgToDelete = "";
        int messagesDeletedCounter = 0;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Trivia Bot");
            Console.WriteLine("==================================");
            TriviaBot crap = new TriviaBot();
            crap.start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        void stackEm(String input1)
        {

            for (int i = 0; i < 74; i++)
            {
                recentMessages[i + 1] = recentMessages[i];
            }
            recentMessages[0] = input1;
        }

        public string getScores()
        {
            string[] Entry = File.ReadAllLines("c:\\triviabot\\scores.txt");
            var orderedEntries = Entry.OrderByDescending(x => int.Parse(x.Split(',')[1]));

            var myList = orderedEntries.Take(5);
            String highScores = "High scores: ";
            foreach (var score in myList)
            {
                highScores += score + " | ";
            }

            return highScores;
        }

        void addQuestion(String newQuestion)
        {
            int counter = 0;
            string myLine = "";
            string question = "";
            string answer = "";
            System.IO.StreamReader file =
            new System.IO.StreamReader("c:\\triviabot\\questions.txt");

            while ((myLine = file.ReadLine()) != null)
            {
                counter++;
            }
            string[] values = newQuestion.Split('#');
            question = values[0];
            answer = values[1];
            question = question.Remove(0, 5);
            file.Close();
            File.AppendAllText("c:\\triviabot\\answers.txt", answer + Environment.NewLine);
            File.AppendAllText("c:\\triviabot\\questions.txt", question + Environment.NewLine);
        }

        static void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit] = newText;
            File.WriteAllLines(fileName, arrLine);
        }

        public int getUserScore(String userName)
        {
            int theirScore = 0;
            int counter = 0;
            string myLine = "";
            System.IO.StreamReader file =
            new System.IO.StreamReader("c:\\triviabot\\scores.txt");

            while ((myLine = file.ReadLine()) != null)
            {
                if (myLine.Contains(userName))
                {
                    string[] values = myLine.Split(',');
                    Int32.TryParse(values[1], out theirScore);
                }
                counter++;
            }
            file.Close();

            return theirScore;
        }

        public void addPoint(String userName)
        {
            int temp2 = 0;
            int counter = 0;
            int foundLine = -1;
            string myLine = "";
            String replacement = "";
            String newUserLine = "";
            int destinationLine = 0;
            System.IO.StreamReader file =
            new System.IO.StreamReader("c:\\triviabot\\scores.txt");

            while ((myLine = file.ReadLine()) != null)
            {
                if (myLine.Contains(userName))
                {

                    foundLine = counter;
                    string[] values = myLine.Split(',');
                    Int32.TryParse(values[1], out temp2);
                    temp2++;
                    replacement = values[0] + "," + temp2;

                }

                counter++;
            }
            file.Close();

            if (foundLine == -1)
            {
                newUserLine = userName + "," + "1";
                destinationLine = counter + 1;
                File.AppendAllText("c:\\triviabot\\scores.txt", newUserLine + Environment.NewLine);
            }
            else
            {
                lineChanger(replacement, "c:\\triviabot\\scores.txt", foundLine);
                foundLine = -1;
            }
        }

        public void generateHints()
        {

            StringBuilder sb = new StringBuilder(currentAnswer);

            for (int i = 1; i < sb.Length - 1; i++)
            {
                sb[i] = '_';
            }
            hint1 = sb.ToString();
        }

        public Boolean checkSpam(String msgToCheck)
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
        public void getQuestion()
        {
            var lines = File.ReadAllLines("c:\\triviabot\\questions.txt");
            var r = new Random();
            var randomLineNumber = r.Next(0, lines.Length - 1);
            var line = lines[randomLineNumber];
            currentQuestion = line;
            currentQuestionLine = randomLineNumber;
        }

        public void getAnswer()
        {
            var answerLines = File.ReadAllLines("c:\\triviabot\\answers.txt");
            currentAnswer = answerLines[currentQuestionLine];

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
        void Timer5_Tick(object state)
        {
            try
            {
                getMsg1(currentAnswer);
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
        void Timer6_Tick(object state)
        {
            try
            {
                getMsg2(currentAnswer);
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
        void Timer7_Tick(object state)
        {
            try
            {
                getMsg4(currentAnswer);
                //Console.WriteLine(DateTime.Now + " getting new messages");
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
        void Timer8_Tick(object state)
        {
            try
            {
                getMsg5(currentAnswer);
              //  Console.WriteLine(DateTime.Now + " getting new messages");
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

        void Timer4_Tick(object state)
        {
            try
            {
                //temporarily disabled while testing
                //sendMsg("Commands:  !trivia  !stop  !myscore  !info KDCA  !wx KDCA");
                //Console.WriteLine("Timer4");

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
        void Timer2_Tick(object state)
        {
            Console.WriteLine("Timer2");
            if (done == 1)
            {
                Console.WriteLine("Timer2 done true");
                getQuestion();
                getAnswer();
                generateHints();
                stage = 0;
                done = 0;
                questionAnswered = 0;
            }
            else if (stage == 0 && done != 1)
            {
                Console.WriteLine("Timer2 done false");
                stage++;
                askTime = DateTime.Now;
                try
                {
                    msgToSend = currentQuestion + "?";

                    sendMsg(msgToSend);


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
            else if (stage == 1 && done != 1)
            {
                Console.WriteLine("Timer2 state 1 true not done");
                stage++;
                try
                {
                    //  sendMsg("hint 1");
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
            else if (stage == 2 && done != 1)
            {
                Console.WriteLine("Timer2 stage 2 not done");
                stage++;
                try
                {

                    sendMsg("Hint: " + hint1);


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
            else if (stage == 3 && done != 1)
            {
                stage++;
                try
                {
                    //  sendMsg("hint 3");
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
            else if (stage == 4 && done != 1)
            {
                stage = 0;
                done = 1;
                try
                {
                    sendMsg("Time is up! The correct answer was:  " + currentAnswer);


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
        }

        private void start()
        {
            Console.WriteLine("start function");
            Timer1 = new Timer(Timer1_Tick, null, 34000, 300000);  // Delay for retrieving channel chat messages. Changed from 2000 to 60000
            Timer3 = new Timer(Timer3_Tick, null, 15000, 15000);
            Timer4 = new Timer(Timer4_Tick, null, 30000, 3600000);
            Timer5 = new Timer(Timer5_Tick, null, 30000, 300000);  // Delay for retrieving channel chat messages. cam 1
            Timer6 = new Timer(Timer6_Tick, null, 32000, 300000);  // Delay for retrieving channel chat messages. cam 2
            Timer7 = new Timer(Timer7_Tick, null, 36000, 300000);  // Delay for retrieving channel chat messages. cam 4
            Timer8 = new Timer(Timer8_Tick, null, 38000, 300000);  // Delay for retrieving channel chat messages. cam 5
            Console.WriteLine("start function done");
        }

        public async Task sendMsg(string myMessage)
        {
            Console.WriteLine("sendmsg function");
            if (startUpMsgHoldBack == 0)
            {
                Console.WriteLine("SENT " + myMessage);
                UserCredential credential;

                using (var stream = new FileStream("c:\\triviabot\\client_secrets.json", FileMode.Open, FileAccess.Read))
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

            using (var stream = new FileStream("c:\\triviabot\\client_secrets.json", FileMode.Open, FileAccess.Read))
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
               // Console.Write(" 1 ");
                var now = DateTime.Now;
                var timeSince = now - messageTime;
                int toSeconds = timeSince.Seconds;
                // Console.WriteLine(DateTime.Now + "   msg time: " + messageTime + "  ago: " + timeSince);

                // && toSeconds < 33 && toSeconds > 25 
                if (displayName != "Trivia Bot" && recentMessages.Contains(messageId).Equals(false) && startUpMsgHoldBack == 0)
                {
                    // stackEm(messageId);
                    // Console.WriteLine("recent message: " + messageTime + " Delay: " + toSeconds + "  " + displayMessage);

                    // if (displayMessage.Contains(curAnswer) && done == 0 && questionAnswered == 0)
                    if ((displayMessage.IndexOf(curAnswer, StringComparison.OrdinalIgnoreCase) >= 0) && done == 0 && questionAnswered == 0)
                    {
                        questionAnswered = 1;
                        done = 1;
                        String output1 = "You got it, " + displayName + "! [" + toSeconds + "secs] The correct answer was: " + curAnswer + ".";
                        sendMsg(output1);
                        addPoint(displayName);
                    }
                    else

                    if (displayMessage.Contains("!trivia"))
                    {
                        done = 1;
                        stage = 0;// necessary?
                        String msg = "Trivia Bot started! First question coming up...";
                        sendMsg(msg);
                        Timer2 = new Timer(new TimerCallback(Timer2_Tick), null, 0, 10000);
                    }
                    else

                    if (displayMessage.Contains("!stop"))
                    {
                        done = 1;
                        String msg = "Trivia Stopped by " + displayName;
                        sendMsg(msg);
                        Timer2.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    else

                    if (displayMessage.Contains("!myscore"))
                    {
                        int s1 = getUserScore(displayName);
                        String msg = displayName + "'s score: " + s1;
                        Console.WriteLine("Display name: " + displayName);
                        sendMsg(msg);
                    }
                    else

                    if (displayMessage.Contains("!add"))
                    {
                        addQuestion(displayMessage);
                        String msg = "Question added.";
                        sendMsg(msg);
                    }
                    else

                    if (displayMessage.Contains("!info"))
                    {
                        Airport a = new Airport();
                        string t1 = a.getAirportInfo(displayMessage);
                        sendMsg(t1);
                    }
                    else

                    if (displayMessage.Contains("!highscores"))
                    {
                        string t4 = getScores();
                        sendMsg(t4);
                    }
                    else

                    if (displayMessage.Contains("!wx") || displayMessage.Contains("!weather"))
                    {
                        AirportWx b = new AirportWx();
                        string t2 = b.getAirportWx(displayMessage);
                        sendMsg(t2);
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

            using (var stream = new FileStream("c:\\triviabot\\client_secrets.json", FileMode.Open, FileAccess.Read))
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

        public async Task getMsg1(String curAnswer)
        {
            Console.WriteLine(DateTime.Now + "Getting channel 1 messages.");
            UserCredential credential;

            using (var stream = new FileStream("c:\\triviabot\\client_secrets.json", FileMode.Open, FileAccess.Read))
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
            chatMessages.PageToken = nextpagetoken1;
            var chatResponse = await chatMessages.ExecuteAsync();
            nextpagetoken1 = chatResponse.NextPageToken;
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
               // Console.Write(" 1 ");
                var now = DateTime.Now;
                var timeSince = now - messageTime;
                int toSeconds = timeSince.Seconds;
                //Console.WriteLine(DateTime.Now + "   msg time: " + messageTime + "  ago: " + timeSince);

                // && toSeconds < 33 && toSeconds > 25 
                if (displayName != "Trivia Bot" && recentMessages.Contains(messageId).Equals(false) && startUpMsgHoldBack == 0)
                {
                    // stackEm(messageId);
                    //Console.WriteLine("recent message: " + messageTime + " Delay: " + toSeconds + "  " + displayMessage);

                    // if (displayMessage.Contains(curAnswer) && done == 0 && questionAnswered == 0)
                    if (checkSpam(displayMessage) == true) await deleteMsg(messageId, displayMessage, 1);
                }
            }
        }


        public async Task getMsg2(String curAnswer)
        {
            Console.WriteLine("Getting channel 2 messages.");
            UserCredential credential;

            using (var stream = new FileStream("c:\\triviabot\\client_secrets.json", FileMode.Open, FileAccess.Read))
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
            chatMessages.PageToken = nextpagetoken2;
            var chatResponse = await chatMessages.ExecuteAsync();
            nextpagetoken2 = chatResponse.NextPageToken;
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
               // Console.Write(" 1 ");
                var now = DateTime.Now;
                var timeSince = now - messageTime;
                int toSeconds = timeSince.Seconds;
                //Console.WriteLine(DateTime.Now + "   msg time: " + messageTime + "  ago: " + timeSince);

                // && toSeconds < 33 && toSeconds > 25 
                if (displayName != "Trivia Bot" && recentMessages.Contains(messageId).Equals(false) && startUpMsgHoldBack == 0)
                {
                    // stackEm(messageId);
                    //Console.WriteLine("recent message: " + messageTime + " Delay: " + toSeconds + "  " + displayMessage);

                    // if (displayMessage.Contains(curAnswer) && done == 0 && questionAnswered == 0)

                    if (checkSpam(displayMessage) == true) await deleteMsg(messageId, displayMessage, 2);

                }
            }
        }
        public async Task getMsg4(String curAnswer)
        {
            Console.WriteLine("Getting channel 4 messages.");
            UserCredential credential;

            using (var stream = new FileStream("c:\\triviabot\\client_secrets.json", FileMode.Open, FileAccess.Read))
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
            chatMessages.PageToken = nextpagetoken4;
            var chatResponse = await chatMessages.ExecuteAsync();
            nextpagetoken4 = chatResponse.NextPageToken;
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
               // Console.Write(" 1 ");
                var now = DateTime.Now;
                var timeSince = now - messageTime;
                int toSeconds = timeSince.Seconds;
                //Console.WriteLine(DateTime.Now + "   msg time: " + messageTime + "  ago: " + timeSince);

                // && toSeconds < 33 && toSeconds > 25 
                if (displayName != "Trivia Bot" && recentMessages.Contains(messageId).Equals(false) && startUpMsgHoldBack == 0)
                {
                    // stackEm(messageId);
                    // Console.WriteLine("recent message: " + messageTime + " Delay: " + toSeconds + "  " + displayMessage);

                    // if (displayMessage.Contains(curAnswer) && done == 0 && questionAnswered == 0)

                    if (checkSpam(displayMessage) == true) await deleteMsg(messageId, displayMessage, 4);

                }
            }
        }

        public async Task getMsg5(String curAnswer)
        {
            Console.WriteLine("Getting channel 5 messages.");
            UserCredential credential;

            using (var stream = new FileStream("c:\\triviabot\\client_secrets.json", FileMode.Open, FileAccess.Read))
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
            chatMessages.PageToken = nextpagetoken5;
            var chatResponse = await chatMessages.ExecuteAsync();
            nextpagetoken5 = chatResponse.NextPageToken;
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
               // Console.Write(" 1 ");
                var now = DateTime.Now;
                var timeSince = now - messageTime;
                int toSeconds = timeSince.Seconds;
               // Console.WriteLine(DateTime.Now + "   msg time: " + messageTime + "  ago: " + timeSince);

                // && toSeconds < 33 && toSeconds > 25 
                if (displayName != "Trivia Bot" && recentMessages.Contains(messageId).Equals(false) && startUpMsgHoldBack == 0)
                {
                    // stackEm(messageId);
                    //Console.WriteLine("recent message: " + messageTime + " Delay: " + toSeconds + "  " + displayMessage);

                    // if (displayMessage.Contains(curAnswer) && done == 0 && questionAnswered == 0)

                    if (checkSpam(displayMessage) == true) await deleteMsg(messageId, displayMessage, 5);

                }
            }
        }
    }
}