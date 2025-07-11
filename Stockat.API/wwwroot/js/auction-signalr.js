// Auction SignalR Client
class AuctionSignalRClient {
    constructor(connectionUrl, accessToken) {
        this.connectionUrl = connectionUrl;
        this.accessToken = accessToken;
        this.connection = null;
        this.isConnected = false;
    }

    async startConnection() {
        try {
            // Create SignalR connection with authentication
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(this.connectionUrl, {
                    accessTokenFactory: () => this.accessToken
                })
                .withAutomaticReconnect()
                .build();

            // Set up event handlers
            this.setupEventHandlers();

            // Start the connection
            await this.connection.start();
            this.isConnected = true;
            console.log('Connected to Auction Hub');
            
            // Trigger connected event
            this.onConnected();
        } catch (error) {
            console.error('Error connecting to Auction Hub:', error);
            this.onError(error);
        }
    }

    setupEventHandlers() {
        // Connection events
        this.connection.onclose((error) => {
            this.isConnected = false;
            console.log('Disconnected from Auction Hub');
            this.onDisconnected(error);
        });

        this.connection.onreconnecting((error) => {
            console.log('Reconnecting to Auction Hub...');
            this.onReconnecting(error);
        });

        this.connection.onreconnected((connectionId) => {
            this.isConnected = true;
            console.log('Reconnected to Auction Hub');
            this.onReconnected(connectionId);
        });

        // Auction events
        this.connection.on('Connected', (connectionId) => {
            console.log('Connected with ID:', connectionId);
        });

        this.connection.on('JoinedAuction', (auctionId) => {
            console.log('Joined auction:', auctionId);
            this.onJoinedAuction(auctionId);
        });

        this.connection.on('LeftAuction', (auctionId) => {
            console.log('Left auction:', auctionId);
            this.onLeftAuction(auctionId);
        });

        this.connection.on('BidPlaced', (data) => {
            console.log('New bid placed:', data);
            this.onBidPlaced(data);
        });

        this.connection.on('BidSuccess', (bid) => {
            console.log('Bid successful:', bid);
            this.onBidSuccess(bid);
        });

        this.connection.on('AuctionUpdate', (data) => {
            console.log('Auction updated:', data);
            this.onAuctionUpdate(data);
        });

        this.connection.on('AuctionStarted', (data) => {
            console.log('Auction started:', data);
            this.onAuctionStarted(data);
        });

        this.connection.on('AuctionEndingSoon', (data) => {
            console.log('Auction ending soon:', data);
            this.onAuctionEndingSoon(data);
        });

        this.connection.on('AuctionClosed', (data) => {
            console.log('Auction closed:', data);
            this.onAuctionClosed(data);
        });

        this.connection.on('Error', (message) => {
            console.error('SignalR Error:', message);
            this.onError(message);
        });
    }

    // Join an auction room
    async joinAuction(auctionId) {
        if (!this.isConnected) {
            throw new Error('Not connected to SignalR');
        }
        console.log('Joining auction:', auctionId);
        await this.connection.invoke('JoinAuction', auctionId);
    }

    // Leave an auction room
    async leaveAuction(auctionId) {
        if (!this.isConnected) {
            throw new Error('Not connected to SignalR');
        }
        await this.connection.invoke('LeaveAuction', auctionId);
    }

    // Place a bid through SignalR
    async placeBid(auctionId, bidAmount) {
        if (!this.isConnected) {
            throw new Error('Not connected to SignalR');
        }
        
        const bidDto = {
            auctionId: auctionId,
            bidAmount: bidAmount
        };
        
        await this.connection.invoke('PlaceBid', bidDto);
    }

    // Get auction updates
    async getAuctionUpdates(auctionId) {
        if (!this.isConnected) {
            throw new Error('Not connected to SignalR');
        }
        await this.connection.invoke('GetAuctionUpdates', auctionId);
    }

    // Watch an auction
    async watchAuction(auctionId) {
        if (!this.isConnected) {
            throw new Error('Not connected to SignalR');
        }
        await this.connection.invoke('WatchAuction', auctionId);
    }

    // Stop watching an auction
    async stopWatchingAuction(auctionId) {
        if (!this.isConnected) {
            throw new Error('Not connected to SignalR');
        }
        await this.connection.invoke('StopWatchingAuction', auctionId);
    }

    // Event handlers - override these in your implementation
    onConnected() {
        // Override this method to handle connection
    }

    onDisconnected(error) {
        // Override this method to handle disconnection
    }

    onReconnecting(error) {
        // Override this method to handle reconnecting
    }

    onReconnected(connectionId) {
        // Override this method to handle reconnection
    }

    onJoinedAuction(auctionId) {
        // Override this method to handle joining auction
    }

    onLeftAuction(auctionId) {
        // Override this method to handle leaving auction
    }

    onBidPlaced(data) {
        // Override this method to handle new bid
        // data contains: { bid, auction, message, timestamp }
    }

    onBidSuccess(bid) {
        // Override this method to handle successful bid
    }

    onAuctionUpdate(data) {
        // Override this method to handle auction updates
        // data contains: { auction, bids, timestamp }
    }

    onAuctionStarted(data) {
        // Override this method to handle auction start
        // data contains: { auction, message, timestamp }
    }

    onAuctionEndingSoon(data) {
        // Override this method to handle auction ending soon
        // data contains: { auction, message, timestamp }
    }

    onAuctionClosed(data) {
        // Override this method to handle auction close
        // data contains: { auction, winnerId, winningBid, message, timestamp }
    }

    onError(error) {
        // Override this method to handle errors
    }

    // Disconnect from SignalR
    async stopConnection() {
        if (this.connection) {
            await this.connection.stop();
            this.isConnected = false;
        }
    }
}

// Usage example:
/*
// Initialize the client
const auctionClient = new AuctionSignalRClient('/auctionhub', 'your-jwt-token');

// Override event handlers
auctionClient.onBidPlaced = (data) => {
    // Update UI with new bid
    updateAuctionDisplay(data.auction);
    showNotification(data.message);
};

auctionClient.onAuctionClosed = (data) => {
    // Handle auction closure
    showAuctionResult(data);
};

// Start connection
await auctionClient.startConnection();

// Join an auction
await auctionClient.joinAuction(123);

// Place a bid
await auctionClient.placeBid(123, 100.50);
*/ 