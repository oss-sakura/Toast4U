using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Toast4U
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!(args.Length == 2 || args.Length == 3))
            {
                Usage();
                return;
            }

            if (args.Length == 3 && !(File.Exists(args[2])))
            {
                Console.Error.WriteLine("{0} image file not found!", args[2]);
                return;
            }

            Toaster toaster = new Toaster();
            Task.Run(() => toaster.SendToastNotification(args));

            while (toaster.Finished == false)
            {
                Thread.Sleep(10);
            }
        }

        static void Usage()
        {
            string command = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
            Console.WriteLine("Usage: {0} title message [image]", command);
            Console.WriteLine("  e.g.: {0} INFOMATION \"HELLO WORLD\"", command);
            Console.WriteLine("  e.g.: {0} INFOMATION \"HELLO WORLD\" c:\\path\\to\\image.png", command);
        }
    }

    class Toaster
    {
        private ToastNotification _toast;
        public bool Finished { get; private set; } = false;

        public void SendToastNotification(string[] args)
        {
            string Toast = "";
            string title = args[0];

            switch (args.Length)
            {
                case 2:
                    // Title and Message XML
                    Toast = String.Format(
                        "<toast><visual><binding template=\"ToastText01\"><text id=\"1\">{0}</text></binding></visual></toast>"
                        ,args[1]);
                    break;
                case 3:
                    // Title, Message and Image XML
                    Toast = String.Format(
                        "<toast><visual><binding template=\"ToastImageAndText01\"><image id=\"1\" src=\"{1}\"/><text id=\"1\">{0}</text></binding></visual></toast>"
                        , args[1], args[2]);
                    break;
                default:
                    // unknown case
                    Toast = "<toast><visual><binding template=\"ToastText01\"><text id=\"1\">unknown</text></binding></visual></toast>";
                    break;
            }

            XmlDocument toastTemplate = new XmlDocument();
            toastTemplate.LoadXml(Toast);

            // Create the toast and attach event handlers.
            _toast = new ToastNotification(toastTemplate);
            _toast.Dismissed += ToastNotificationComplete;
            _toast.Activated += ToastNotificationComplete;
            _toast.Failed += ToastNotificationComplete;

            ToastNotificationManager.CreateToastNotifier(title).Show(_toast);
        }

        private void ToastNotificationComplete(ToastNotification sender, object args)
        {
            Finished = true;
        }
    }
}
