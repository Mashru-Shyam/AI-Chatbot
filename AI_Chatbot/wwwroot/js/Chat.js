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
        const messageHtml = `<div class="message ${messageClass}">${message}</div>`;
        chatMessages.append(messageHtml);
        chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, 'slow');
    }

    function processUserInput(input) {
        const jwtToken = sessionStorage.getItem('jwtToken') || ''; // Retrieve token from sessionStorage

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
                    addMessage('Login Successful...', 'bot');
                } else {
                    addMessage(response, 'bot'); // Display bot's response
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    addMessage("Unauthorized access. Please log in.", 'bot');
                } else {
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
