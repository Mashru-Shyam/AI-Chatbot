// Toggle chatbot visibility
document.getElementById('toggleChatBtn').addEventListener('click', function () {
    const chatContainer = document.querySelector('.chat-container');
    chatContainer.style.display = chatContainer.style.display === 'block' ? 'none' : 'block';
});

// Chat input functionality
$(document).ready(function () {
    const chatMessages = $('#chatMessages');
    const userMessageInput = $('#userMessage');

    function addMessage(message, sender) {
        const messageClass = sender === 'user' ? 'user' : 'bot';
        const formattedMessage = message
            .replace(/\*\*(.*?)\*\*/g, '<b>$1</b>') // Bold (**text**)
            .replace(/_(.*?)_/g, '<i>$1</i>')       // Italics (_text_)
            .replace(/~~(.*?)~~/g, '<s>$1</s>')     // Strikethrough (~~text~~)
            .replace(/`([^`]+)`/g, '<code>$1</code>') // Inline code (`text`)
            .replace(/\[(.*?)\]\((.*?)\)/g, '<a href="$2" target="_blank">$1</a>') // Hyperlinks [text](url)
            .replace(/\n/g, '<br>');                // Newline (\n)
        const messageHtml = `<div class="message ${messageClass}">${formattedMessage}</div>`;
        chatMessages.append(messageHtml);
        chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, 'slow');
    }

    function showTypingIndicator() {    
        const typingHtml = `
            <div class="message bot typing-indicator">
                Typing
                <span class="dot"></span>
                <span class="dot"></span>
                <span class="dot"></span>
            </div>`;
        chatMessages.append(typingHtml);
        chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, 'slow');
    }
    
    function removeTypingIndicator() {
        chatMessages.find('.typing-indicator').remove();
    }
    


    function processUserInput(input) {
        const jwtToken = sessionStorage.getItem('jwtToken') || ''; // Retrieve token from sessionStorage

        showTypingIndicator();

        $.ajax({
            url: `https://localhost:7048/api/Chat/send-message`,
            method: "POST",
            headers: {
                'Authorization': `Bearer ${jwtToken}`,
            },
            contentType: "application/json",
            data: JSON.stringify({ message: input }),
            success: function (response) {
                if (response.token) {
                    sessionStorage.setItem('jwtToken', response.token);
                    removeTypingIndicator();
                    addMessage('Login Successful...', 'bot');
                } else {
                    const formattedResponse = response
                    .replace(/\*\*(.*?)\*\*/g, '<b>$1</b>')   // Bold (**text**)
                    .replace(/_(.*?)_/g, '<i>$1</i>')        // Italics (_text_)
                    .replace(/~~(.*?)~~/g, '<s>$1</s>')      // Strikethrough (~~text~~)
                    .replace(/`([^`]+)`/g, '<code>$1</code>') // Inline Code (`text`)
                    .replace(/\[(.*?)\]\((.*?)\)/g, '<a href="$2" target="_blank">$1</a>') // Links [text](url)
                    .replace(/\n/g, '<br>');                 // Newline (\n)
                    removeTypingIndicator();
                    addMessage(formattedResponse, 'bot'); // Display bot's response
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    removeTypingIndicator();
                    addMessage("Unauthorized access. Please log in.", 'bot');
                } else {
                    removeTypingIndicator();
                    addMessage("There was an error processing your request. Please try again later.", 'bot');
                }
            }
        });
    }

    $('#sendMessage').click(function () {
        const message = userMessageInput.val().trim();
        if (message !== '') {
            addMessage(message, 'user');
            userMessageInput.val('');
            processUserInput(message);
        } else {
            addMessage("Please type a message before sending.", 'bot');
        }
    });

    userMessageInput.keypress(function (e) {
        if (e.which === 13) {
            $('#sendMessage').click();
        }
    });
});
