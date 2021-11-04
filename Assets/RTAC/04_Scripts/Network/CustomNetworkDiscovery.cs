using kcp2k;

using System.Net;
using Mirror;
using Mirror.Discovery;

using System;

using UnityEngine;
using UnityEngine.Events;

using Random = UnityEngine.Random;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-discovery
    API Reference: https://mirror-networking.com/docs/api/Mirror.Discovery.NetworkDiscovery.html
*/
namespace AltarChase.Networking
{
    public class DiscoveryRequest : NetworkMessage
    {
        // Add properties for whatever information you want sent by clients
        // in their broadcast messages that servers will consume.
    }

    public class DiscoveryResponse : NetworkMessage
    {
        // Add properties for whatever information you want the server to return to
        // clients for them to display or consume for establishing a connection.
        
        // the server that sent the message
        // This is a property that is not serialized but the client fills this up after we recieved it.
        public IPEndPoint EndPoint { get; set; }

        public Uri uri;

        public ushort port;
        
        public long serverId;
    }

    [Serializable] public class ServerFoundEvent : UnityEvent<DiscoveryResponse>{}
    
    public class CustomNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
    {

    #region Server

        public long ServerId { get; private set; }

        [Tooltip("Transport to be advertised during discovery.")]
        public Transport transport;

        [Tooltip("Invoked when a server is found")] public ServerFoundEvent onServerFound = new ServerFoundEvent();
        
        public override void Start()
        {
            ServerId = RandomLong();
            // If the transport wasn't set in the inspector, use the active one.
            // transport.activeTransport is set in Awake of Transport components.
            if(transport == null)
            {
                transport = Transport.activeTransport;
            }
            
            base.Start();
        }

        /// <summary>
        /// Reply to the client to inform it of this server
        /// </summary>
        /// <remarks>
        /// Override if you wish to ignore server requests based on
        /// custom criteria such as language, full server game mode or difficulty
        /// </remarks>
        /// <param name="request">Request coming from client</param>
        /// <param name="endpoint">Address of the client that sent the request</param>
        protected override DiscoveryResponse ProcessRequest(DiscoveryRequest _request, IPEndPoint _endPoint)
        {
            try
            {
                // this is just an example reply message, you could add anything here about the match.
                // i.e. the game name, game mode, host name, language, ping
                // this has a chance to throw an exception if the transport doesn't support network discovery.
                return new DiscoveryResponse()
                {
                    serverId = ServerId, 
                    uri = transport.ServerUri(),
                    port = ((KcpTransport)transport).Port
                };
            }
            catch(NotImplementedException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Create a message that will be broadcasted on the network to discover servers
        /// </summary>
        /// <remarks>
        /// Override if you wish to include additional data in the discovery message
        /// such as desired game mode, language, difficulty, etc... </remarks>
        /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
        protected override DiscoveryRequest GetRequest() => new DiscoveryRequest();
        
    #endregion

    #region Client

        /// <summary>
        /// Process the answer from a server
        /// </summary>
        /// <remarks>
        /// A client receives a reply from a server, this method processes the
        /// reply and raises an event
        /// </remarks>
        /// <param name="_response">Response that came from the server</param>
        /// <param name="_endpoint">Address of the server that replied</param>
        protected override void ProcessResponse(DiscoveryResponse _response, IPEndPoint _endpoint)
        {
            //james doesn't fully understand this code, but knows that it is nessecary to work

        #region WTF

            // we recieved a message from te remote endpoint
            _response.EndPoint = _endpoint;
            
            //although we got a supposedly valid url we may not be able to resolve the provided host
            // however we know the real ip address of the server because we just recieved a packet from it,
            // so use that as the host uri
            UriBuilder realUri = new UriBuilder(_response.uri)
            {
                Host = _response.EndPoint.Address.ToString()
            };
            _response.uri = realUri.Uri;

        #endregion
        }

    #endregion
    }
}


