# Real-Time Auction System with SignalR

This implementation adds real-time functionality to the auction system using SignalR. Users can now receive live updates when bids are placed, auctions start/end, and other auction-related events.

## Features

### Real-Time Events
- **Bid Placed**: Notifies all users watching an auction when a new bid is placed
- **Auction Started**: Notifies users when an auction begins
- **Auction Ending Soon**: Warns users when an auction is about to end (5 minutes before)
- **Auction Closed**: Notifies users when an auction ends and announces the winner
- **Auction Updates**: Provides real-time updates of auction status and bid history

### SignalR Hub Methods

#### Client to Server
- `JoinAuction(auctionId)`: Join an auction room to receive updates
- `LeaveAuction(auctionId)`: Leave an auction room
- `PlaceBid(bidDto)`: Place a bid through SignalR (alternative to REST API)
- `GetAuctionUpdates(auctionId)`: Request current auction data
- `WatchAuction(auctionId)`: Start watching an auction
- `StopWatchingAuction(auctionId)`: Stop watching an auction

#### Server to Client Events
- `BidPlaced`: New bid placed on auction
- `BidSuccess`: Confirmation of successful bid
- `AuctionUpdate`: Updated auction information
- `AuctionStarted`: Auction has started
- `AuctionEndingSoon`: Auction is ending soon
- `AuctionClosed`: Auction has ended with winner
- `JoinedAuction`: Confirmation of joining auction
- `LeftAuction`: Confirmation of leaving auction
- `Error`: Error messages

## Backend Implementation

### Files Added/Modified

1. **`Stockat.API/Hubs/AuctionHub.cs`**
   - Main SignalR hub for auction real-time functionality
   - Handles bid placement, auction joining/leaving, and notifications

2. **`Stockat.API/Services/AuctionNotificationService.cs`**
   - Service for sending real-time notifications
   - Can be used by both controllers and background services

3. **`Stockat.API/Services/AuctionMonitorService.cs`**
   - Background service that monitors auctions
   - Sends notifications for auction start/end events
   - Runs every minute to check auction status

4. **`Stockat.API/Controllers/AuctionBidRequestController.cs`**
   - Modified to send real-time notifications when bids are created via REST API

5. **`Stockat.API/Program.cs`**
   - Added SignalR hub mapping
   - Registered notification and monitor services

6. **`Stockat.API/wwwroot/js/auction-signalr.js`**
   - JavaScript client for connecting to SignalR hub
   - Includes all event handlers and methods

## Frontend Integration

### 1. Include SignalR Client Library

Add the SignalR client library to your HTML:

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.min.js"></script>
<script src="/js/auction-signalr.js"></script>
```

### 2. Initialize the Client

```javascript
// Get JWT token from your authentication system
const token = localStorage.getItem('jwt_token');

// Initialize the auction client
const auctionClient = new AuctionSignalRClient('/auctionhub', token);

// Override event handlers
auctionClient.onBidPlaced = (data) => {
    // Update UI with new bid information
    updateCurrentBid(data.auction.currentBid);
    updateBidHistory(data.bid);
    showNotification(data.message);
};

auctionClient.onAuctionClosed = (data) => {
    // Handle auction closure
    showAuctionResult(data);
    disableBidding();
};

auctionClient.onAuctionEndingSoon = (data) => {
    // Show warning that auction is ending soon
    showEndingSoonWarning(data.message);
};

// Start the connection
await auctionClient.startConnection();
```

### 3. Join an Auction

```javascript
// When user views an auction page
await auctionClient.joinAuction(auctionId);
```

### 4. Place a Bid

You can place bids either through the REST API or directly through SignalR:

```javascript
// Option 1: Through SignalR (recommended for real-time experience)
await auctionClient.placeBid(auctionId, bidAmount);

// Option 2: Through REST API (will still trigger real-time updates)
const response = await fetch('/api/AuctionBidRequest', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
        auctionId: auctionId,
        bidAmount: bidAmount
    })
});
```

### 5. Clean Up

```javascript
// When leaving auction page
await auctionClient.leaveAuction(auctionId);

// When closing application
await auctionClient.stopConnection();
```

## Configuration

### Authentication

The SignalR hub requires authentication. Make sure your JWT token is included in the connection:

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/auctionhub', {
        accessTokenFactory: () => token
    })
    .build();
```

### CORS Configuration

If your frontend is on a different domain, ensure CORS is properly configured in `Program.cs`:

```csharp
builder.Services.ConfigureCors();
```

## Background Services

The `AuctionMonitorService` runs every minute to:

1. Check for auctions that have just started
2. Check for auctions ending soon (within 5 minutes)
3. Close ended auctions and notify users
4. Send real-time notifications for all events

## Error Handling

The implementation includes comprehensive error handling:

- Connection failures are handled gracefully
- Automatic reconnection is enabled
- Errors are logged but don't break the application
- Failed notifications don't prevent bid creation

## Performance Considerations

- SignalR uses WebSockets when available, falling back to other transports
- Automatic reconnection handles network issues
- Background service runs every minute (configurable)
- Notifications are sent asynchronously to avoid blocking

## Testing

### Test Real-Time Updates

1. Open multiple browser tabs/windows
2. Join the same auction in each tab
3. Place a bid in one tab
4. Verify all tabs receive the real-time update

### Test Background Events

1. Create an auction that starts in the near future
2. Wait for the start notification
3. Create an auction that ends in the near future
4. Wait for the ending soon and closed notifications

## Troubleshooting

### Common Issues

1. **Connection fails**: Check JWT token and authentication
2. **No real-time updates**: Ensure you've joined the auction room
3. **Background events not firing**: Check the monitor service logs
4. **CORS errors**: Verify CORS configuration

### Debug Mode

Enable detailed logging in the browser console:

```javascript
auctionClient.onError = (error) => {
    console.error('SignalR Error:', error);
    // Show user-friendly error message
};
```

## Security Considerations

- All hub methods require authentication
- Users can only join auctions they have access to
- Bid validation is performed server-side
- Rate limiting should be implemented for production use 