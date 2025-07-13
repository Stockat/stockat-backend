# OpenAI Integration for Stockat Chatbot

## Overview

This integration adds OpenAI GPT-3.5-turbo capabilities to the Stockat chatbot, providing more intelligent and contextual responses while maintaining fallback to the existing platform data-based responses.

## Features

- **OpenAI GPT-3.5-turbo Integration**: Uses OpenAI's API for intelligent responses
- **Chat History Context**: Includes previous conversation context for better responses
- **Fallback System**: Falls back to platform data-based responses if OpenAI fails
- **Configurable Settings**: Easy configuration through appsettings.json
- **Error Handling**: Comprehensive error handling and logging

## Setup Instructions

### 1. Get OpenAI API Key

1. Go to [OpenAI Platform](https://platform.openai.com/)
2. Create an account or sign in
3. Navigate to API Keys section
4. Create a new API key
5. Copy the API key (you'll need it for configuration)

### 2. Configure API Key

Update the `appsettings.json` file in the `Stockat.API` project:

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_ACTUAL_OPENAI_API_KEY_HERE",
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 800,
    "Temperature": 0.7
  }
}
```

**Important**: Replace `YOUR_ACTUAL_OPENAI_API_KEY_HERE` with your actual OpenAI API key.

### 3. Configuration Options

| Setting | Description | Default | Recommended |
|---------|-------------|---------|-------------|
| `ApiKey` | Your OpenAI API key | Required | Your OpenAI API key |
| `Model` | OpenAI model to use | gpt-3.5-turbo | gpt-3.5-turbo or gpt-4 |
| `MaxTokens` | Maximum tokens in response | 800 | 800-1000 |
| `Temperature` | Response creativity (0-1) | 0.7 | 0.7-0.9 |

### 4. Environment Variables (Optional)

For production environments, you can use environment variables instead of appsettings.json:

```bash
# Set environment variables
export OpenAI__ApiKey="your-api-key-here"
export OpenAI__Model="gpt-3.5-turbo"
export OpenAI__MaxTokens="800"
export OpenAI__Temperature="0.7"
```

## Architecture

### Components

1. **IOpenAIService**: Interface for OpenAI operations
2. **OpenAIService**: Implementation of OpenAI API calls
3. **AIService**: Enhanced with OpenAI integration and fallback
4. **ChatBotController**: Updated to use OpenAI with chat history
5. **OpenAIConfigs**: Configuration class for OpenAI settings

### Flow

1. User sends message to `/api/chatbot/ask`
2. Message is saved to chat history
3. Previous chat history is retrieved (last 10 messages)
4. OpenAI service is called with context and history
5. If OpenAI fails, falls back to platform data-based responses
6. Response is saved to chat history
7. Response is returned to user

## API Endpoints

### POST /api/chatbot/ask

Send a message to the chatbot with OpenAI integration.

**Request:**
```json
{
  "message": "What are the top sellers?",
  "includeServiceSuggestions": false,
  "additionalContext": {}
}
```

**Response:**
```json
{
  "response": "Based on our platform data, here are the top sellers...",
  "timestamp": "2024-01-01T12:00:00Z",
  "userId": "user-id"
}
```

## Error Handling

The integration includes comprehensive error handling:

- **API Key Missing**: Throws configuration error on startup
- **OpenAI API Errors**: Logs errors and falls back to platform data
- **Network Issues**: Handles timeouts and connection problems
- **Rate Limiting**: Handles OpenAI rate limits gracefully

## Logging

All OpenAI interactions are logged:

- **Info**: Successful API calls and responses
- **Warning**: Fallback to platform data
- **Error**: API errors and exceptions

## Security Considerations

1. **API Key Protection**: Never commit API keys to source control
2. **Environment Variables**: Use environment variables in production
3. **Rate Limiting**: Monitor API usage to avoid excessive costs
4. **Input Validation**: All user inputs are validated before sending to OpenAI

## Cost Management

OpenAI API usage incurs costs based on:

- **Model Used**: gpt-3.5-turbo is cheaper than gpt-4
- **Token Usage**: Both input and output tokens count
- **Request Frequency**: More requests = higher costs

**Tips for cost management:**
- Use appropriate `MaxTokens` setting
- Monitor usage in OpenAI dashboard
- Consider implementing request caching
- Set up usage alerts in OpenAI

## Testing

### Test the Integration

1. Start the application
2. Send a POST request to `/api/chatbot/ask`
3. Verify OpenAI responses are received
4. Test fallback by temporarily using an invalid API key

### Sample Test Requests

```bash
# Test basic functionality
curl -X POST http://localhost:5250/api/chatbot/ask \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, what can you help me with?"}'

# Test with context
curl -X POST http://localhost:5250/api/chatbot/ask \
  -H "Content-Type: application/json" \
  -d '{"message": "Tell me about top sellers"}'
```

## Troubleshooting

### Common Issues

1. **"OpenAI API key not configured"**
   - Check appsettings.json has correct API key
   - Verify API key is valid in OpenAI dashboard

2. **"OpenAI API error: 401"**
   - Invalid API key
   - Check API key permissions

3. **"OpenAI API error: 429"**
   - Rate limit exceeded
   - Wait and retry, or upgrade OpenAI plan

4. **Fallback to platform data**
   - Check logs for specific OpenAI errors
   - Verify network connectivity
   - Check OpenAI service status

### Debug Steps

1. Check application logs for OpenAI errors
2. Verify API key in OpenAI dashboard
3. Test API key with curl or Postman
4. Check network connectivity to OpenAI
5. Verify configuration in appsettings.json

## Future Enhancements

1. **Caching**: Implement response caching to reduce API calls
2. **Streaming**: Add streaming responses for real-time chat
3. **Fine-tuning**: Custom model training for domain-specific responses
4. **Multi-language**: Support for multiple languages
5. **Analytics**: Track conversation quality and user satisfaction

## Support

For issues with the OpenAI integration:

1. Check the application logs
2. Verify OpenAI API key and configuration
3. Test with OpenAI's API directly
4. Review OpenAI's documentation for API changes 