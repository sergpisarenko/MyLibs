using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using SnowLib.Config;

namespace SnowLib.WCF
{
    /// <summary>
    /// Класс, реализющий создание и подготовку подключения к сервису 
    /// на основе строки параметров подключения
    /// </summary>
    public static class ServiceClient
    {
        private const int defaultTimeoutSeconds = 10;
        private const int defaultMaxReceivedMessageSize = 8388608;
        private const int defaultMaxBufferSize = 8388608;
        private const int defaultMaxStringContentLength = 8388608;
        private const int defaultMaxArrayLength = 8388608;
        private const int defaultMaxBytesPerRead = 8388608;

        // for windows security issue see http://blogs.msdn.com/b/tiche/archive/2011/07/13/wcf-on-intranet-with-windows-authentication-kerberos-or-ntlm-part-1.aspx
        // The problem is as following. Kerberos auth needs service principal name or user principal name. 
        // If is not set, WCF extracs host name from Uri. But, if service host runs under use domain/accout
        // WCF cannot read machine principal name and throws error "A call to SSPI failed,  The target principal name is incorrect".
        // WCF doens't automatically fallbacks to NTLM from Kerberos in this case.
        // Solutions:
        // 1) On client side set dummy SPN name a priori incorrect. (null, String.Empty):
        // EndpointAddress ep = new EndpointAddress(new Uri(ServiceUri), EndpointIdentity.CreateSpnIdentity(String.Empty));
        // Incorrect SPN name alway fallbacks WCF into NTLM.
        // 2) Set service principal name for domain account:
        // setspn.exe –U –A MySystem/Service1 DOMAIN\SERVICEACCOUNT (requires special rights)
        // On client side set correct SPN name:
        // EndpointAddress ep = new EndpointAddress(new Uri(ServiceUri), EndpointIdentity.CreateSpnIdentity("MySystem/Service1"));
        // 3) Set user principal name (UPN) on client side:
        // EndpointAddress ep = new EndpointAddress(new Uri(ServiceUri), EndpointIdentity.CreateUpnIdentity("DOMAIN\\SERVICEACCOUNT"));
        //
        // Imperonation:
        // Need client setup:
        // factory.Credentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation (or delegation)
        // on server side use method attribure:
        // [OperationBehavior(Impersonation = ImpersonationOption.Required)]
        // for multi hip-hop delegation (client-appserver-sqlserver on different machines)
        // see http://www.codeproject.com/Articles/38979/How-to-enable-multi-hop-impersonation-using-constr
        public static T Get<T>(string connectionString)
        {
            PropertyString connectionProperties = new PropertyString(connectionString);
            bool isWindowsCredential;
            NetTcpBinding binding = getBinding(connectionProperties, out isWindowsCredential);
            EndpointAddress endPointAddress = getEndPoint(connectionProperties, isWindowsCredential);
            ChannelFactory<T> factory = new ChannelFactory<T>(binding, endPointAddress);
            setupFactory(factory, connectionProperties, isWindowsCredential);
            return factory.CreateChannel();
        }

        public static T Get<T>(string connectionString, object callbackImplementation, IEndpointBehavior behavior = null)
        {
            PropertyString connectionProperties = new PropertyString(connectionString);
            bool isWindowsCredential;
            NetTcpBinding binding = getBinding(connectionProperties, out isWindowsCredential);
            EndpointAddress endPointAddress = getEndPoint(connectionProperties, isWindowsCredential);
            InstanceContext instanceContext = new InstanceContext(callbackImplementation);
            DuplexChannelFactory<T> factory = new DuplexChannelFactory<T>(instanceContext, binding, endPointAddress);
            if (behavior != null)
                factory.Endpoint.EndpointBehaviors.Add(behavior);
            setupFactory(factory, connectionProperties, isWindowsCredential);
            return factory.CreateChannel();
        }


        private static NetTcpBinding getBinding(PropertyString connectionProperties, out bool isWindowsCredential)
        {
            SecurityMode securityMode = connectionProperties.GetEnum<SecurityMode>("SecurityMode", true);
            NetTcpBinding binding = new NetTcpBinding(securityMode, false);
            binding.OpenTimeout = connectionProperties.GetOptional("OpenTimeout", TimeSpan.FromSeconds(defaultTimeoutSeconds));
            binding.CloseTimeout = connectionProperties.GetOptional("CloseTimeout", TimeSpan.FromSeconds(defaultTimeoutSeconds));
            binding.ReceiveTimeout = connectionProperties.GetOptional("RecieveTimeout", TimeSpan.FromSeconds(defaultTimeoutSeconds));
            binding.SendTimeout = connectionProperties.GetOptional("SendTimeout", TimeSpan.FromSeconds(defaultTimeoutSeconds));
            binding.MaxReceivedMessageSize = connectionProperties.GetOptional("MaxReceivedMessageSize", defaultMaxReceivedMessageSize);
            binding.MaxBufferSize = connectionProperties.GetOptional("MaxBufferSize", defaultMaxBufferSize);
            binding.ReaderQuotas.MaxStringContentLength = connectionProperties.GetOptional("MaxStringContentLength", defaultMaxStringContentLength);
            binding.ReaderQuotas.MaxArrayLength = connectionProperties.GetOptional("MaxArrayLength", defaultMaxArrayLength);
            binding.ReaderQuotas.MaxBytesPerRead = connectionProperties.GetOptional("MaxBytesPerRead", defaultMaxBytesPerRead);
            switch (securityMode)
            {
                case SecurityMode.Message:
                    binding.Security.Message.ClientCredentialType = connectionProperties.GetEnum<MessageCredentialType>("MessageCredentialType", false);
                    isWindowsCredential = binding.Security.Message.ClientCredentialType == MessageCredentialType.Windows;
                    break;

                case SecurityMode.Transport:
                    binding.Security.Transport.ClientCredentialType = connectionProperties.GetEnum<TcpClientCredentialType>("ClientCredentialType", false);
                    isWindowsCredential = binding.Security.Transport.ClientCredentialType == TcpClientCredentialType.Windows;
                    break;

                default:
                    isWindowsCredential = false;
                    break;
            }
            return binding;
        }

        private static EndpointAddress getEndPoint(PropertyString connectionProperties, bool isWindowsCredential)
        {
            EndpointAddress endPointAddress;
            Uri uri = new Uri(connectionProperties.GetString("Uri", true));
            if (isWindowsCredential)
            {
                string upnIdentity = connectionProperties.GetString("UPNIdentity", false);
                if (!String.IsNullOrEmpty(upnIdentity))
                    endPointAddress = new EndpointAddress(uri, EndpointIdentity.CreateUpnIdentity(upnIdentity));
                else
                {
                    string spnIdentity = connectionProperties.GetString("SPNIdentity", false);
                    if (!String.IsNullOrEmpty(spnIdentity))
                        endPointAddress = new EndpointAddress(uri, EndpointIdentity.CreateSpnIdentity(upnIdentity));
                    else
                        endPointAddress = new EndpointAddress(uri);
                }
            }
            else
                endPointAddress = new EndpointAddress(uri);
            return endPointAddress;
        }

        private static void setupFactory(ChannelFactory factory, PropertyString connectionProperties, bool isWindowsCredential)
        {
            if (isWindowsCredential)
            {
                bool allowNtml = connectionProperties.GetOptional("AllowNtml", true);
                factory.Credentials.Windows.AllowNtlm = connectionProperties.GetOptional("AllowNtlm", allowNtml);
            }
            System.Security.Principal.TokenImpersonationLevel tokenImpersonationLevel;
            if (!connectionProperties.GetEnum<System.Security.Principal.TokenImpersonationLevel>("TokenImpersonationLevel", out tokenImpersonationLevel))
            {
                tokenImpersonationLevel = isWindowsCredential ?
                    System.Security.Principal.TokenImpersonationLevel.Impersonation : System.Security.Principal.TokenImpersonationLevel.None;
            }
            factory.Credentials.Windows.AllowedImpersonationLevel = tokenImpersonationLevel;
        }

        public static void Close(object client)
        {
            ICommunicationObject commObj = client as ICommunicationObject;
            if (commObj != null)
            {
                if (commObj.State == CommunicationState.Faulted)
                    commObj.Abort();
                else
                    try
                    {
                        commObj.Close();
                    }
                    catch
                    {
                        commObj.Abort();
                    }
            }
        }
    }
}
