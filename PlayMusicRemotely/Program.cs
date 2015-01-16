using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using NetduinoLibrary.Toolbox;
using System.Reflection;
using System.IO;
using System.Resources;
using SecretLabs.NETMF.IO;

namespace PlayMusicRemotely
{
    public class Program
    {
        static WebServer server = new WebServer(80, 1000);
        public static void Main()
        {
            server.CommandReceived += server_CommandReceived;
            server.Start();
            Thread.Sleep(Timeout.Infinite);
        }

        


        static void server_CommandReceived(object obj, WebServer.WebServerEventArgs e)
        {
            Socket response = e.response;

            string strResp = "";

            string rawUrl = e.rawURL;

            strResp = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n";
            strResp = WebServer.OutPutStream(response, strResp);

            //rawUrl will be something like "req.aspx?note=C1C1C1g1a1a1g2E1E1D1D1C2"
            if(rawUrl.IndexOf("note=") != -1)
            {
                string note = rawUrl.Substring(rawUrl.IndexOf("=") + 1);
                PlayMusic(note);
                return;
            }

            strResp += "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";

            strResp += "<html xmlns=\"http://www.w3.org/1999/xhtml\"><head><title>Playing Music</title>";
            
            strResp += "<meta http-equiv=\"Cache-control\" content=\"no-cache\"/>";

            strResp += "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/></head><body>";


            //create the script part

            strResp += "<script language=\"JavaScript\">var xhr = new XMLHttpRequest();function btnclicked(boxMSG, cmdSend) {";

            strResp += "document.getElementById('status').innerHTML=\"playing music\";";

            strResp += "xhr.open('GET', cmdSend + boxMSG.value);";

            strResp += "xhr.send(null); xhr.onreadystatechange = function() {if (xhr.readyState == 4) {document.getElementById('status').innerHTML=xhr.responseText;}};}";

            strResp += "</script>";

            strResp += "<table>";

            strResp += "<tr><td><input type=\"text\" value=\"C1C1C1g1a1a1g2E1E1D1D1C2\" id=\"note\" /></td></tr>";

            strResp += "<tr><td><input type=\"button\" value=\"Play Music\" id=\"noteButton\" onclick=\"btnclicked(document.getElementById('note'),'req.aspx?note=')\" /></td></tr>";

            strResp += "</table><div id=\"status\"></div></body></html>";

            strResp = WebServer.OutPutStream(response, strResp);

        }



        public static void PlayMusic(string song = "C1C1C1g1a1a1g2E1E1D1D1C2")
        {
            System.Collections.Hashtable scale = new System.Collections.Hashtable();
            // low octave
            scale.Add("c", 1915u);
            scale.Add("d", 1700u);
            scale.Add("e", 1519u);
            scale.Add("f", 1432u);
            scale.Add("g", 1275u);
            scale.Add("a", 1136u);
            scale.Add("b", 1014u);
            // high octave
            scale.Add("C", 956u);
            scale.Add("D", 851u);
            scale.Add("E", 758u);
            // silence ("hold note")
            scale.Add("h", 0u);

            int beatsPerMinute = 90;
            int beatTimeInMilliseconds =
             60000 / beatsPerMinute; // 60,000 milliseconds per minute
            int pauseTimeInMilliseconds = (int)(beatTimeInMilliseconds * 0.1);

            // define the song (letter of note followed by length of note)
            
#if MF_FRAMEWORK_VERSION_V4_3
            SecretLabs.NETMF.Hardware.PWM speaker =
                new SecretLabs.NETMF.Hardware.PWM(Pins.GPIO_PIN_D5);
#else
         Microsoft.SPOT.Hardware.PWM pwm = 
            new Microsoft.SPOT.Hardware.PWM(PWMChannels.PWM_PIN_D5, 20000, 800, Microsoft.SPOT.Hardware.PWM.ScaleFactor.Microseconds, false);
#endif

            for (int i = 0; i < song.Length; i += 2)
            {
                // song loop
                // extract each note and its length in beats
                string note = song.Substring(i, 1);
                int beatCount = int.Parse(song.Substring(i + 1, 1));

                uint noteDuration = (uint)scale[note];

                // play the note for the desired number of beats
#if MF_FRAMEWORK_VERSION_V4_3

                speaker.SetPulse(noteDuration * 2, noteDuration);
#else
                    pwm.Duration = noteDuration;
                    pwm.Period = noteDuration * 2;  
#endif

                Thread.Sleep(
                 beatTimeInMilliseconds * beatCount - pauseTimeInMilliseconds);

                // pause for 1/10th of a beat in between every note.
                speaker.SetDutyCycle(0);
                Thread.Sleep(pauseTimeInMilliseconds);
            }
            
        }

    }
}
