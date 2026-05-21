# AI Healthcare Chatbot - Intellichat

An intelligent healthcare chatbot that leverages natural language processing to assist patients with appointment booking, prescription management, and medical inquiries.

**Live Demo:** https://intellichat-bsdec5ajd5c3gfh4.westindia-01.azurewebsites.net/

## 📋 Project Overview

Intellichat is a healthcare-focused chatbot application that understands user intent through natural language processing. It provides two-tier access:

- **Public Access (No Login Required):** Answer general health questions and provide information
- **Authenticated Access (Login Required):** Book appointments, view appointment details, manage prescriptions

## ✨ Features

- **Natural Language Understanding:** Detects and interprets user queries in English
- **Appointment Management:** Book and view appointments
- **Prescription Tracking:** Access and manage prescriptions
- **User Authentication:** Secure login for personal health information
- **Intelligent Responses:** Context-aware chatbot responses based on user intent
- **User Logic:** Personalized user management system built into the chatbot

## 🛠️ Tech Stack

- **Backend:** C# (84.7%)
- **Frontend:** JavaScript (6.5%), HTML (3.6%), CSS (5.2%)

## 🚀 Getting Started

### Prerequisites
- .NET Framework / .NET Core
- Node.js (for frontend dependencies)
- Database (SQL Server / similar)

### Installation

1. Clone the repository
   ```bash
   git clone https://github.com/Mashru-Shyam/AI-Chatbot.git
   cd AI-Chatbot
   ```

2. Setup backend
   ```bash
   cd backend
   dotnet restore
   dotnet build
   ```

3. Setup frontend
   ```bash
   cd frontend
   npm install
   ```

4. Run the application
   ```bash
   dotnet run
   ```

## 💬 Usage

### General Questions (No Login)
- "What are the symptoms of flu?"
- "Tell me about common cold"

### Authenticated Features (Login Required)
- "Book an appointment with Dr. Smith"
- "Show my appointments"
- "What are my current prescriptions?"

## ⚕️ Medical Disclaimer

This chatbot is designed to provide general health information and appointment management assistance. **It is not a substitute for professional medical advice, diagnosis, or treatment.** Always consult with a qualified healthcare provider for medical concerns.

## 📄 License

No License

---

Built with ❤️ for better healthcare accessibility
