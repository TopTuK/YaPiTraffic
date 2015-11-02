using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml;

namespace YaTraffic
{
    public class TrafficEventArgs : EventArgs
    {
        public readonly int Level;
        public readonly string Title;

        public TrafficEventArgs(int level, string title) :
            base()
        {
            Level = level;
            Title = title;
        }
    }

    public sealed class YaTrafficManager
    {
        private const string XML_URL = @"http://export.yandex.ru/bar/reginfo.xml";

        private int m_level;
        private string m_title;
        private readonly Object m_lockObject = new Object();

        public event EventHandler<TrafficEventArgs> OnDataChanged = null;

        public YaTrafficManager()
        {
            m_level = -1;
            m_title = @"Unknown";
        }

        public void UpdateData()
        {
            HttpWebRequest webRequest = WebRequest.CreateHttp(XML_URL);
            webRequest.Proxy = null;
            webRequest.BeginGetResponse(new AsyncCallback(OnRequestComplete), webRequest);
        }

        // Этот метод будет вызван в отдельном потоке!
        private void OnRequestComplete(IAsyncResult reqResult)
        {
            HttpWebRequest webRequest = reqResult.AsyncState as HttpWebRequest;
            HttpWebResponse webResponse = webRequest.EndGetResponse(reqResult) as HttpWebResponse;
            if (webResponse != null)
            {
                using (webResponse)
                {
                    Stream xmlStream = webResponse.GetResponseStream();
                    ParseXMLData(xmlStream);
                }
            }
        }

        private void ParseXMLData(Stream xmlStream)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlStream);

            XmlElement xmlTraffic = xmlDoc.DocumentElement["traffic"];
            if (xmlTraffic != null)
            {
                string level = xmlTraffic["level"].FirstChild.Value;
                string title = xmlTraffic["title"].FirstChild.Value;

                int iLevel;
                if (!int.TryParse(level, out iLevel)) iLevel = -1;

                if (title == null) title = @"Unknown";

                if((Level != iLevel) || (Title != title))
                {
                    Level = iLevel;
                    Title = title;

                    if (OnDataChanged != null) OnDataChanged(this, new TrafficEventArgs(iLevel, title));
                }
            }
        }

        public int Level
        {
            get
            {
                int result = -1;
                lock(m_lockObject)
                {
                    result = m_level;
                }
                return result;
            }
            private set
            {
                lock(m_lockObject)
                {
                    m_level = value;
                }

            }
        }

        public string Title
        {
            get
            {
                string result = null;
                lock(m_lockObject)
                {
                    result = m_title;
                }
                return result;
            }
            private set
            {
                lock(m_lockObject)
                {
                    m_title = value;
                }
            }
        }
    }
}
