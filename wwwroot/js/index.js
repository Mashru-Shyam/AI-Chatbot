//Toogle the chatbot
document.getElementById("toggleChatBtn").addEventListener("click", function () {
  const chatContainer = document.querySelector(".chat-container");
  chatContainer.style.display =
    chatContainer.style.display === "inline-block" ? "none" : "inline-block";
});

$(document).ready(function () {
  const chatMessages = $("#chatMessages");
  const userMessageInput = $("#userMessage");
  let value = "General";

  //Starting messgae
  addMessage(
    "Hello! Welcome to IntelliChat. How can I assist you today? 😊",
    "bot"
  );
  showButtons();

  //Message fuction to add a user or bot message
  function addMessage(message, sender) {
    const messageClass = sender === "user" ? "user" : "bot";
    const formattedMessage = message
      .replace(/\*\*(.*?)\*\*/g, "<b>$1</b>")
      .replace(/_(.*?)_/g, "<i>$1</i>")
      .replace(/~~(.*?)~~/g, "<s>$1</s>")
      .replace(/`([^`]+)`/g, "<code>$1</code>")
      .replace(/\[(.*?)\]\((.*?)\)/g, '<a href="$2" target="_blank">$1</a>')
      .replace(/\n/g, "<br>");
    const messageHtml = `<div class="message ${messageClass}">${formattedMessage}</div>`;
    chatMessages.append(messageHtml);
    chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, "slow");
  }

  //Removing typing...
  function removeTypingIndicator() {
    chatMessages.find(".typing-indicator").remove();
  }

  //Removing action buttons
  function removeActionButtons() {
    chatMessages.find(".action-buttons").remove();
  }

  //Showing typing...
  function showTypingIndicator() {
    const typingHtml = `
            <div class="message bot typing-indicator">
                Typing
                <span class="dot"></span>
                <span class="dot"></span>
                <span class="dot"></span>
            </div>`;
    chatMessages.append(typingHtml);
    chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, "slow");
  }

  //Show user and general buttons
  function showButtons() {
    const buttonsHtml = `
            <div class="action-buttons">
                <button id="general">General Conversation</button>
                <button id="user">User Conversation</button>
            </div>`;
    chatMessages.append(buttonsHtml);
    chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, "slow");
    bindGeneralAndUserButtons();
  }

  //Showing appointment, payment, insurance, prescription buttons
  function showOnLoginButtons() {
    const buttonsHtml = `
            <div class="action-buttons">
                <button id="schedule">Schedule Appointment 📅</button>
                <button id="appointment">Check Appointments 📆</button>
                <button id="payment">Payment Info 💳</button>
                <button id="prescription">My Prescriptions 💊</button>
                <button id="insurance">Insurance Info 🏥</button>
                <button id="general">Ask Anything ❓</button>
            </div>`;

    chatMessages.append(buttonsHtml);
    chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, "slow");
    bindOnLoginButtons();
  }

  //Date and Time Picker Buttons
  function showDateButtons() {
    const buttonsHtml = `
            <div class="action-buttons">
                <input type="date" id="date" placeholder="Select a Date 📅">
            </div>
        `;
    chatMessages.append(buttonsHtml);
    chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, "slow");
    bindDateButtons();
    }
    function showTimeButtons() {
        const buttonsHtml = `
            <div class="action-buttons">
                <input type="time" id="time" placeholder="Select a Time ⏰">
            </div>
        `;
        chatMessages.append(buttonsHtml);
        chatMessages.animate({ scrollTop: chatMessages[0].scrollHeight }, "slow");
        bindTimeButtons();
    }

  //Adding date and time functionalities
  function bindDateButtons() {
    const dateInput = document.getElementById("date");

    dateInput.setAttribute("min", new Date().toISOString().split("T")[0]);

    //Function to check if both date and time are provided
    function checkInputs() {
      if (dateInput.value) {
        const selectedDate = new Date(dateInput.value);
        const formattedDate = `${selectedDate
          .getDate()
          .toString()
          .padStart(2, "0")}/${(selectedDate.getMonth() + 1)
          .toString()
          .padStart(2, "0")}/${selectedDate
          .getFullYear()
          .toString()
          .slice(-2)}`;

        $(".chat-input").removeClass("disabled");
        removeActionButtons();
        handleUserMessage(
          `Date: ${formattedDate}`
        );
      } else {
        $(".chat-input").addClass("disabled");
      }
    }

    dateInput.addEventListener("change", checkInputs);
  }

    function bindTimeButtons() {
        const timeInput = document.getElementById("time");

        //Function to check if both date and time are provided
        function checkInputs() {
            if (timeInput.value) {
                let [hours, minutes] = timeInput.value.split(":");
                const period = hours >= 12 ? "PM" : "AM";
                hours = hours % 12 || 12;
                const formattedTime = `${hours}:${minutes} ${period}`;
                $(".chat-input").removeClass("disabled");
                removeActionButtons();
                handleUserMessage(
                    `Time: ${formattedTime}`
                );
            } else {
                $(".chat-input").addClass("disabled");
            }
        }

        timeInput.addEventListener("change", checkInputs);
    }

  //User and general button click functionalities
  function bindGeneralAndUserButtons() {
    $("#general").click(function () {
      value = "General";
      removeActionButtons();
      addMessage("Type your query below ✍️", "bot");
    });

    $("#user").click(function () {
      value = "User";
      removeActionButtons();
      addMessage("How can I help you today? 😊", "bot");
      showOnLoginButtons();
    });
  }

  //login buttons functionalities
  function bindOnLoginButtons() {
    $("#schedule").click(function () {
      handleUserMessage("Schedule Appointment");
    });

    $("#appointment").click(function () {
      handleUserMessage("Check Appointments");
    });

    $("#payment").click(function () {
      handleUserMessage("Payment Info");
    });

    $("#prescription").click(function () {
      handleUserMessage("My Prescriptions");
    });

    $("#insurance").click(function () {
      handleUserMessage("Insurance Info");
    });

    $("#general").click(function () {
      value = "General";
      removeActionButtons();
      addMessage("Type your query below ✍️", "bot");
    });
  }

  //Handle the message - adding message and processing input
  function handleUserMessage(message) {
    removeActionButtons();
    addMessage(message, "user");
    processUserInput(message);
  }

  //Process the input - backed integration response
  function processUserInput(input) {
      showTypingIndicator();

      const apiBaseUrl = window.location.hostname === "localhost"
          ? "https://localhost:7048"
          : "https://intellichat-bsdec5ajd5c3gfh4.westindia-01.azurewebsites.net";

    $.ajax({
        url: `${apiBaseUrl}/api/Chating/send-message`,
      method: "POST",
      contentType: "application/json",
      data: JSON.stringify(input),
      success: function (response) {
        removeTypingIndicator();

        if (response.query) {
          addMessage(response.query, "bot");
          showOnLoginButtons();
        } else if (response.date) {
          addMessage(response.date, "bot");
          showDateButtons();
        } else if (response.time) {
            addMessage(response.time, "bot");
            showTimeButtons();
        } else if (response.otpEmail) {
          addMessage(response.otpEmail, "bot");
        } else {
          addMessage(response, "bot");
          if (value == "General") {
            showButtons();
          } else {
            showOnLoginButtons();
          }
        }
      },
      error: function (xhr) {
        removeTypingIndicator();
        if (xhr.status === 401) {
            addMessage("Access denied! Please log in to continue. 🔒", "bot");
            if (value == "General") {
                showButtons();
            } else {
                showOnLoginButtons();
            }
        } else {
          addMessage(
            "Oops! Something went wrong. Please try again later. 🔄",
            "bot"
            );
            if (value == "General") {
                showButtons();
            } else {
                showOnLoginButtons();
            }
        }
      },
    });
  }

  //Send button click functionality
  $("#sendMessage").click(function () {
    removeActionButtons();
    const message = userMessageInput.val().trim();
    if (message !== "") {
      addMessage(message, "user");
      userMessageInput.val("");
      processUserInput(message);
    } else {
      addMessage(
        "Type your message and hit 'Send' to continue. 📩",
        "bot"
        );
      if (value == "General") {
            showButtons();
          } else {
            showOnLoginButtons();
          }
    }
  });

  //Enter key press functionality
  userMessageInput.keypress(function (e) {
    if (e.which === 13) {
      $("#sendMessage").click();
    }
  });
});
