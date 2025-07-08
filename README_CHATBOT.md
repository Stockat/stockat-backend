# Stockat Chatbot Implementation

## Overview

The Stockat chatbot is an AI-powered assistant that helps users find information about products, services, sellers, auctions, and more on the B2B manufacturing platform.

## Architecture

### Backend Components

1. **ChatBotController** (`/api/chatbot`)
   - Handles chatbot interactions
   - Endpoints:
     - `POST /api/chatbot/ask` - Send a message to the chatbot
     - `GET /api/chatbot/history` - Get chat history

2. **Services**
   - `IChatHistoryService` & `ChatHistoryService` - Manages chat message history
   - `IAIService` & `AIService` - Handles AI response generation
   - `IOrderService` & `OrderService` - Provides top sellers data
   - Enhanced `IProductService` & `ProductService` - Provides product and auction data
   - Enhanced `IServiceService` & `ServiceService` - Provides service data

3. **DTOs**
   - `ChatRequestDto` - Request model for chatbot messages

### Frontend Components

1. **ChatbotComponent** - Angular component for the chatbot interface
   - Modern, responsive chat UI
   - Real-time message handling
   - Loading states and error handling

## Features

### Current Capabilities

1. **Product Information**
   - Get top-selling products
   - Product category statistics
   - Product details and availability

2. **Seller Information**
   - Top sellers based on activity
   - Seller performance metrics

3. **Auction Information**
   - Live auctions
   - Auction status and details

4. **Service Information**
   - Popular services
   - Service categories and details

5. **General Platform Information**
   - Platform overview
   - Help and guidance

### AI Response System

The chatbot uses a keyword-based response system that can be easily extended or replaced with a more sophisticated AI service like OpenAI, Azure OpenAI, or other AI providers.

## API Endpoints

### POST /api/chatbot/ask
Send a message to the chatbot.

**Request:**
```json
{
  "message": "What are the top sellers?"
}
```

**Response:**
```json
{
  "response": "Based on our platform data, I can help you find top sellers..."
}
```

### GET /api/chatbot/history
Get chat history for the authenticated user.

**Query Parameters:**
- `limit` (optional): Number of messages to retrieve (default: 50)

**Response:**
```json
[
  {
    "content": "What are the top sellers?",
    "senderId": "user-id",
    "timestamp": "2024-01-01T12:00:00Z",
    "role": "user"
  },
  {
    "content": "Based on our platform data...",
    "senderId": "system",
    "timestamp": "2024-01-01T12:00:01Z",
    "role": "assistant"
  }
]
```

## Integration with AI Services

To integrate with a real AI service (like OpenAI), replace the `AIService` implementation:

```csharp
public class OpenAIAService : IAIService
{
    private readonly OpenAIClient _openAi;
    
    public OpenAIAService(OpenAIClient openAi)
    {
        _openAi = openAi;
    }
    
    public async Task<string> GenerateResponseAsync(string userMessage, object contextData)
    {
        var systemPrompt = CreateSystemPrompt(contextData);
        
        var response = await _openAi.ChatCompletion.CreateAsync(new()
        {
            Model = "gpt-4o",
            Messages = new[]
            {
                new ChatMessage("system", systemPrompt),
                new ChatMessage("user", userMessage)
            },
            MaxTokens = 800,
            Temperature = 0.7
        });
        
        return response.Choices[0].Message.Content;
    }
}
```

## Database Schema

The chatbot uses the existing `ChatMessage` entity for storing conversation history. Messages are stored with:
- `Content`: The message text
- `SenderId`: User ID for user messages, "system" for AI responses
- `Timestamp`: When the message was sent
- `MessageType`: Text message type

## Security

- All endpoints require authentication
- User messages are associated with their user ID
- Chat history is scoped to the authenticated user

## Future Enhancements

1. **Advanced AI Integration**
   - OpenAI GPT-4 integration
   - Azure OpenAI integration
   - Custom model training

2. **Enhanced Features**
   - File upload support
   - Voice messages
   - Multi-language support
   - Context-aware responses

3. **Analytics**
   - Chat analytics
   - User interaction tracking
   - Response quality metrics

4. **Integration**
   - WebSocket support for real-time chat
   - Push notifications
   - Email integration

## Usage

### Backend
The chatbot is automatically available when the application starts. No additional configuration is required.

### Frontend
To use the chatbot component:

```typescript
import { ChatbotComponent } from './features/chatbot/chatbot.component';

// Add to your routing module
{
  path: 'chatbot',
  component: ChatbotComponent
}
```

## Testing

Test the chatbot with these sample queries:
- "What are the top sellers?"
- "Show me popular products"
- "Are there any live auctions?"
- "What services are available?"
- "Help me find products by category"

## Troubleshooting

1. **Authentication Issues**
   - Ensure the user is authenticated
   - Check JWT token validity

2. **Database Issues**
   - Verify database connection
   - Check entity configurations

3. **AI Service Issues**
   - Verify AI service configuration
   - Check API keys and endpoints

## Contributing

When adding new features to the chatbot:

1. Update the `IAIService` implementation
2. Add new data queries to relevant services
3. Update the frontend component if needed
4. Add appropriate tests
5. Update this documentation 