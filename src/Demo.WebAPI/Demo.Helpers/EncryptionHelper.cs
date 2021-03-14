using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace Demo.Helpers {
    public static class EncryptionHelper {
        public static void FromXmlFile(this RSA rsa, string xmlFilePath) {
            var parameters = new RSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(File.ReadAllText(xmlFilePath));

            if (xmlDoc.DocumentElement != null && xmlDoc.DocumentElement.Name.Equals("RSAKeyValue")) {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes) {
                    switch (node.Name) {
                        case "Modulus":
                            parameters.Modulus = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "Exponent":
                            parameters.Exponent = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "P":
                            parameters.P = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "Q":
                            parameters.Q = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "DP":
                            parameters.DP = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "DQ":
                            parameters.DQ = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "InverseQ":
                            parameters.InverseQ = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "D":
                            parameters.D = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                    }
                }
            } else {
                throw new Exception("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
        }

        public static void ToXmlFile(this RSA rsa, bool includePrivateParameters, string xmlFilePath) {
            var parameters = rsa.ExportParameters(includePrivateParameters);

            File.WriteAllText(xmlFilePath,
                $"<RSAKeyValue><Modulus>{(parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null)}</Modulus><Exponent>{(parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null)}</Exponent><P>{(parameters.P != null ? Convert.ToBase64String(parameters.P) : null)}</P><Q>{(parameters.Q != null ? Convert.ToBase64String(parameters.Q) : null)}</Q><DP>{(parameters.DP != null ? Convert.ToBase64String(parameters.DP) : null)}</DP><DQ>{(parameters.DQ != null ? Convert.ToBase64String(parameters.DQ) : null)}</DQ><InverseQ>{(parameters.InverseQ != null ? Convert.ToBase64String(parameters.InverseQ) : null)}</InverseQ><D>{(parameters.D != null ? Convert.ToBase64String(parameters.D) : null)}</D></RSAKeyValue>"
            );
        }
    }
}