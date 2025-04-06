Moni - Modbus Simulator Web Application

Moni is a full-stack web application built with ASP.NET, C#, and Blazor for managing Modbus devices. The app allows users to interact with a Modbus simulator, perform CRUD operations on SQL-stored devices, and manage their status through Modbus communication.

✨ Features
🔐 User Authentication
Users must register and log in before accessing the application.

When a user registers or logs in, their credentials are stored in the SQL Server database.

A JWT (JSON Web Token) is issued upon login and stored in localStorage to maintain session state across page reloads.

🛠️ Device Management (SQL Database)
Perform CRUD operations (Create, Read, Update, Delete) on devices stored in the SQL Server database.

Edit device names and toggle their status (on/off).

View and manage all SQL-stored device information through the UI.

⚡ Modbus Device Interaction (Simulated Devices)
Modbus devices are not stored in the database and cannot be created or deleted.

You can only change the status (on/off) of Modbus devices through the UI using Modbus TCP communication.

Useful for testing control over Modbus-enabled systems.

📊 Device Table
All devices from the SQL database are shown in a sortable table.

Sort by:

Device ID (ascending)

Name (alphabetically)

Status (on/off or off/on)


🚀 Getting Started
📦 Prerequisites
Make sure you have the following installed:

.NET SDK

Blazor (included in .NET)

SQL Server (for database storage)

Modbus TCP Server (e.g., Modbus Poll Slave or ModbusHD for simulation)

NModbus library (for Modbus communication)

🧭 How to Use the Application
1. Start the Backend
Open the project in Visual Studio or VS Code.

Run the backend project using dotnet run.

This will start the API which handles both SQL database and Modbus server communication.

2. Start the Frontend
The frontend is built with Blazor WebAssembly.

Start the frontend from the same project if integrated, or separately if structured that way.

3. Register and Log In
Upon accessing the app, the user is redirected to register or log in.

After logging in, a JWT is stored in local storage.

Authenticated users gain access to all app features.

4. Using the Interface
Home: Shows an overview of all devices and their current status.

Device Management: Create/edit/delete devices, change names and toggle status.

Modbus Communication: Connect to the simulator, send/receive data, and update device states.

🧪 Modbus Simulation Tips
Run a Modbus TCP server locally on 127.0.0.1:502.

Make sure the simulator has configured registers that match the device addresses used in the app.

Moni uses user-friendly addresses (like 1, 2, 3) instead of raw Modbus addresses (like 40001).