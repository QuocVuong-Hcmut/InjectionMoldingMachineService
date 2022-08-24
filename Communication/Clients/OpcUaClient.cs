using Opc.Ua;
using Opc.Ua.Client;

namespace InjectionMoldingMachineDataAcquisitionService.Communication.Clients;
public class OpcUaClient
{
    public string ServerUrl { get; set; }
    public bool IsConnected => _session is not null && _session.Connected;
    public int KeepAliveInterval { get; set; } = 5000;
    public int ReconnectPeriod { get; set; } = 10000;

    private Session? _session;
    private SessionReconnectHandler? _reconnectHandler;
    private ApplicationConfiguration _configuration;
    private readonly object _lock = new();
    private readonly Action<IList, IList> _validateResponse;
    private readonly List<OpcUaSubscription> _subscriptions = new();
    
    public OpcUaClient(string serverUrl)
    {
        ServerUrl = serverUrl;
        _validateResponse = ClientBase.ValidateResponse;
        _configuration = CreateClientConfiguration();
        _configuration.CertificateValidator.CertificateValidation += CertificateValidation;
    }

    public async Task ConnectAsync()
    {
        if (_session is null || _session.Connected == false)
        {
            EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(ServerUrl, false);

            EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(_configuration);
            ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

            Session session = await Session.Create(
                _configuration,
                endpoint,
                false,
                false,
                _configuration.ApplicationName,
                30 * 60 * 1000,
                new UserIdentity(),
                null
            );

            if (session != null && session.Connected == true)
            {
                _session = session;
                _session.KeepAliveInterval = KeepAliveInterval;
                _session.KeepAlive += new KeepAliveEventHandler(Session_KeepAlive);
            }
        }
    }

    public void Disconnect()
    {
        if (_session != null)
        {
            _session.Close();
            _session.Dispose();
            _session = null;
        }
    }

    private void Session_KeepAlive(Session session, KeepAliveEventArgs e)
    {
        try
        {
            // check for events from discarded sessions.
            if (!Object.ReferenceEquals(session, _session))
            {
                return;
            }

            if (ServiceResult.IsBad(e.Status))
            {
                if (ReconnectPeriod <= 0)
                {
                    Console.WriteLine("KeepAlive status {0}, but reconnect is disabled.", e.Status);
                    return;
                }

                lock (_lock)
                {
                    if (_reconnectHandler is null)
                    {
                        Console.WriteLine($"{ServerUrl}: KeepAlive status {e.Status}, reconnecting in {ReconnectPeriod}ms.");
                        Console.WriteLine($"{ServerUrl}: Reconnecting {e.Status}");
                        _reconnectHandler = new SessionReconnectHandler(true);
                        _reconnectHandler.BeginReconnect(_session, ReconnectPeriod, Client_ReconnectComplete);
                    }
                    else
                    {
                        Utils.LogInfo($"{ServerUrl}: KeepAlive status {e.Status}, reconnect in progress.");
                    }
                }

                return;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message, $"{ServerUrl}: Error in OnKeepAlive.");
        }
    }

    private void Client_ReconnectComplete(object? sender, EventArgs e)
    {
        // ignore callbacks from discarded objects.
        if (!Object.ReferenceEquals(sender, _reconnectHandler))
        {
            return;
        }

        lock (_lock)
        {
            // if session recovered, Session property is null
            if (_reconnectHandler?.Session is null)
            {
                _session = _reconnectHandler?.Session;
            }

            _reconnectHandler?.Dispose();
            _reconnectHandler = null;
        }

        Console.WriteLine($"{ServerUrl}: Reconnected");
    }    

    public OpcUaSubscription Subscribe(int publishingInterval)
    {
        if (_session is null || _session.Connected == false)
        {
            throw new OpcUaConnectionException();
        }

        Subscription subscription = new Subscription(_session.DefaultSubscription)
        {
            PublishingEnabled = true,
            PublishingInterval = publishingInterval
        };

        _session.AddSubscription(subscription);
        subscription.Create();
        var opcUaSubscription = new OpcUaSubscription(subscription);
        _subscriptions.Add(opcUaSubscription);
        return opcUaSubscription;
    }

    private void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
    {
        bool certificateAccepted = true;

        ServiceResult error = e.Error;
        while (error != null)
        {
            error = error.InnerResult;
        }

        e.AcceptAll = certificateAccepted;
    }

    private static ApplicationConfiguration CreateClientConfiguration()
    {
        ApplicationConfiguration configuration = new ApplicationConfiguration();

        configuration.ApplicationName = "May Ep Client";
        configuration.ApplicationType = ApplicationType.Client;
        configuration.ApplicationUri = "urn:MayEpClient";
        configuration.ProductUri = "Cha.MayEpClient";
        configuration.SecurityConfiguration = new SecurityConfiguration();
        configuration.SecurityConfiguration.ApplicationCertificate = new CertificateIdentifier();
        configuration.SecurityConfiguration.ApplicationCertificate.StoreType = CertificateStoreType.Directory;
        configuration.SecurityConfiguration.ApplicationCertificate.StorePath = "CurrentUser\\My";
        configuration.SecurityConfiguration.ApplicationCertificate.SubjectName = configuration.ApplicationName;

        configuration.SecurityConfiguration.TrustedIssuerCertificates.StoreType = CertificateStoreType.Directory;
        configuration.SecurityConfiguration.TrustedIssuerCertificates.StorePath = "CurrentUser\\Root";

        configuration.SecurityConfiguration.TrustedPeerCertificates.StoreType = CertificateStoreType.Directory;
        configuration.SecurityConfiguration.TrustedPeerCertificates.StorePath = "CurrentUser\\Root";

        configuration.SecurityConfiguration.RejectedCertificateStore = new CertificateStoreIdentifier();
        configuration.SecurityConfiguration.RejectedCertificateStore.StoreType = CertificateStoreType.Directory;
        configuration.SecurityConfiguration.RejectedCertificateStore.StorePath = "CurrentUser\\Rejected";
        configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates = false;
        configuration.TransportQuotas = new TransportQuotas();
        configuration.TransportQuotas.OperationTimeout = 600000;
        configuration.TransportQuotas.MaxStringLength = 1048576;
        configuration.TransportQuotas.MaxByteStringLength = 1048576;
        configuration.TransportQuotas.MaxArrayLength = 65535;
        configuration.TransportQuotas.MaxMessageSize = 4194304;
        configuration.TransportQuotas.MaxBufferSize = 65535;
        configuration.TransportQuotas.ChannelLifetime = 300000;
        configuration.TransportQuotas.SecurityTokenLifetime = 3600000;
        configuration.ClientConfiguration = new ClientConfiguration();
        configuration.ClientConfiguration.DefaultSessionTimeout = 360000;
        configuration.Validate(ApplicationType.Client);
        return configuration;
    }
}
