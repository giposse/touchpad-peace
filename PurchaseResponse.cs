using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace TouchpadPeace2010
{
    [Serializable]
    public class PurchaseResponse
    {
        [XmlElement("LicenseData")]
        public string LicenseData { get; set; }

        [XmlAttribute("CrcLicense")]
        public string CrcLicense { get; set; }

        [XmlElement("ErrorInformation")]
        public string errorInformation { get; set; }

        [XmlAttribute("Success")]
        public bool Success { get; set; }

        public string ToXmlString()
        {
            StringBuilder sbReturnValue = new StringBuilder();
            XmlSerializer serializer = new XmlSerializer(typeof(PurchaseResponse));
            StringWriter writer = new StringWriter(sbReturnValue);
            serializer.Serialize(writer, this);
            return sbReturnValue.ToString();
        }

        public static PurchaseResponse FromXmlString(string xmlString)
        {
            StringReader reader = new StringReader(xmlString);
            XmlSerializer serializer = new XmlSerializer(typeof(PurchaseResponse));
            PurchaseResponse returnValue = (PurchaseResponse)serializer.Deserialize(reader);
            return returnValue;
        }
    }
}