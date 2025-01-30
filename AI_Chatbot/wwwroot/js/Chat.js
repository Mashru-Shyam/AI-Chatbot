document.getElementById('toggleChatBtn').addEventListener('click', function () {
    const chatContainer = document.querySelector('.chat-container');
    chatContainer.style.display = chatContainer.style.display === 'block' ? 'none' : 'block';
});

$(document).ready(function () {
    const chatMessages = $('#chatMessages');
    const userMessageInput = $('#userMessage');

    addMessage("Hello! Welcome to Chatbot. Please let me know how I can assist you today.", 'bot');
    showButtons();

    function addMessage(message, sender) {
        const messageClass = sender === 'user' ? 'user' : 'bot';
        const formattedMessage = message
            .replace(/\*\*(.*?)\*\*/g, '<b>$1</b>')
            .replace(/_(.*?)_/g, '<i>$1</i>')
            .replace(/~~(.*?)~~/g, '<s>$1</s>')
            .replace(/`([^`]+)`/g, '<code>$1</code>')
            .replace(/\[(.*?)\]\((.*?)\)/g, '<a href="$2" target="_blank">$1</a>')
            .replace(/\n/g, '<br>');
        const messageHtml = `<div class="message ${messageClass}">${formattedMessage}</div>`;
        chatMessages.append(messageHtml);
        chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, 'slow');
    }

    function removeTypingIndicator() {
        chatMessages.find('.typing-indicator').remove();
    }

    function removeActionButtons() {
        chatMessages.find('.action-buttons').remove();
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

    function showButtons() {
        const buttonsHtml = `
            <div class="message action-buttons">
                <button id="general">General Question Answer</button>
                <button id="user">User Based Conversation</button>
            </div>`;
        chatMessages.append(buttonsHtml);
        chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, 'slow');
        bindGeneralAndUserButtons();
    }

    function showOnLoginButtons() {
        const buttonsHtml = `
            <div class="action-buttons">
                <button id="schedule">Schedule Appointment</button>
                <button id="appointment">View Appointments</button>
                <button id="payment">View Payment Details</button>
                <button id="prescription">View Prescriptions</button>
                <button id="insurance">View Insurance Details</button>
                <button id="general">General Question</button>
            </div>`;
        chatMessages.append(buttonsHtml);
        chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, 'slow');
        bindOnLoginButtons();
    }

    function showDateTimeButtons() {
        const buttonsHtml = `
            <div class="action-buttons">
                <input type="text" id="datepicker" placeholder="Pick a date">
                <select id="timepicker" placeholder="Pick a time">
                </select>
            </div>
        `
        chatMessages.append(buttonsHtml);
        chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, 'slow');
        bindDateTimeButtons();
    }

    function bindDateTimeButtons() {
        var currentDate = new Date();
        var currentTime = currentDate.getHours() + ":" + currentDate.getMinutes();
        $(".chat-input").addClass("disabled"); // Disable chat input initially

        $("#datepicker").datepicker({
            dateFormat: "dd/mm/yy",
            changeMonth: true,
            changeYear: true,
            minDate: currentDate, // Set the minimum date to today
            onSelect: function () {
                setTimePicker(); // Update time picker based on the selected date
            }
        });

        // Initialize time picker
        function setTimePicker() {
            var selectedDate = $("#datepicker").val();
            var minTime = "09:00"; // Default minTime to 9:00 AM
            var timeOptions = [];

            if (selectedDate === currentDate.toLocaleDateString()) {
                // If the selected date is today, start from the current time
                minTime = currentTime;
            }

            // Generate time options from the minTime to 11:59 PM with 30-minute intervals
            var time = new Date("1970-01-01 " + minTime);
            for (var i = 0; i <= 28; i++) { // 28 times for 30-minute intervals until 11:59 PM
                var hour = time.getHours();
                var minute = time.getMinutes();
                var ampm = hour >= 12 ? 'PM' : 'AM';

                // Convert 24-hour format to 12-hour format
                hour = hour % 12;
                hour = hour ? hour : 12; // The hour '0' should be '12'
                var formattedTime = (hour < 10 ? "0" + hour : hour) + ":" + (minute < 10 ? "0" + minute : minute) + " "+ ampm;

                timeOptions.push(formattedTime);
                time.setMinutes(time.getMinutes() + 30); // Increment by 30 minutes
            }

            // Populate the time picker dropdown
            var timepicker = $("#timepicker");
            timepicker.empty();
            timepicker.append('<option value="">Select Time</option>');
            timeOptions.forEach(function (time) {
                timepicker.append('<option value="' + time + '">' + time + '</option>');
            });
        }

        $("#datepicker, #timepicker").on('change', function () {
            let date = $("#datepicker").val();
            let time = $("#timepicker").val();

            // Check if both date and time are selected
            if (date && time) {
                removeActionButtons(); // Assuming this is a function you have defined elsewhere
                $(".chat-input").removeClass("disabled"); // Enable the chat input
                handleUserMessage(`Schedule appointment at Date: ${date} and Time: ${time}`); // Handle user message
            }
        });
    }




    function bindGeneralAndUserButtons() {
        $('#general').click(function () {
            removeActionButtons();
            addMessage("Enter your query below.", 'bot');
        });

        $('#user').click(function () {
            removeActionButtons();
            addMessage("How can I assist you?", 'bot');
            showOnLoginButtons();
        });
    }

    function bindOnLoginButtons() {
        $('#schedule').click(function () {
            handleUserMessage("Schedule an Appointment");
        });

        $('#appointment').click(function () {
            handleUserMessage("View Appointments");
        });

        $('#payment').click(function () {
            handleUserMessage("View Payment Details");
        });

        $('#prescription').click(function () {
            handleUserMessage("View Prescriptions");
        });

        $('#insurance').click(function () {
            handleUserMessage("View Insurance Details");
        });

        $('#general').click(function () {
            removeActionButtons();
            addMessage("Enter your query below.", 'bot');
        });
    }

    function handleUserMessage(message) {
        removeActionButtons();
        addMessage(message, 'user');
        processUserInput(message);
    }

    function processUserInput(input) {
        showTypingIndicator();

        $.ajax({
            url: `https://localhost:7048/api/Chat/send-message`,
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(input),
            success: function (response) {
                removeTypingIndicator();

                if (response.query) {
                    addMessage(response.query, 'bot');
                    showOnLoginButtons();
                } else if (response.dateTime) {
                    addMessage(response.dateTime, 'bot');
                    showDateTimeButtons();
                } else if (response.otpEmail) {
                    addMessage(response.otpEmail, 'bot');
                } else {
                    const formattedResponse = response
                        .replace(/\*\*(.*?)\*\*/g, '<b>$1</b>')
                        .replace(/_(.*?)_/g, '<i>$1</i>')
                        .replace(/~~(.*?)~~/g, '<s>$1</s>')
                        .replace(/`([^`]+)`/g, '<code>$1</code>')
                        .replace(/\[(.*?)\]\((.*?)\)/g, '<a href="$2" target="_blank">$1</a>')
                        .replace(/\n/g, '<br>');
                    addMessage(formattedResponse, 'bot');
                    showOnLoginButtons();
                }
            },
            error: function (xhr) {
                removeTypingIndicator();

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
            addMessage("Please enter your message and press 'Send' to proceed.", 'bot');
        }
    });

    userMessageInput.keypress(function (e) {
        if (e.which === 13) {
            $('#sendMessage').click();
        }
    });
});
