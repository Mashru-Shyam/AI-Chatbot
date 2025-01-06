// Toggle chatbot visibility
document.getElementById('toggleChatBtn').addEventListener('click', function () {
    const chatContainer = document.querySelector('.chat-container');
    chatContainer.style.display = chatContainer.style.display === 'block' ? 'none' : 'block';
});

// Chat input functionality (reuse existing logic from your script)
$(document).ready(function () {
    const chatMessages = $('#chatMessages');
    const userMessageInput = $('#userMessage');
    //const apiBaseUrl = "https://localhost:7048/api";

    function addMessage(message, sender) {
        const messageClass = sender === 'user' ? 'user' : 'bot';
        const messageHtml = `<div class="message ${messageClass}">${message}</div>`;
        chatMessages.append(messageHtml);
        chatMessages.scrollTop(chatMessages[0].scrollHeight);
    }

    function processUserInput(input) {
        $.ajax({
            url: `https://localhost:7048/api/Chat/chat`,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ message: input }),
            success: function (response) {
                addMessage(`${response}`, 'bot');
            },
            error: function () {
                addMessage("There was an error processing your request. Please try again later.", 'bot');
            }
        });
    }

    $('#sendMessage').click(function () {
        const message = userMessageInput.val().trim();
        if (message !== '') {
            addMessage(message, 'user');
            userMessageInput.val('');
            processUserInput(message);
        }
    });

    userMessageInput.keypress(function (e) {
        if (e.which === 13) {
            $('#sendMessage').click();
        }
    });
});