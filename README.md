Moni - Modbus Simulator Web Application
Moni is a web application built for managing Modbus devices using C# and Blazor. It allows users to interact with Modbus simulators, perform CRUD operations on devices, and manage values stored in a SQL database.

Features
User Authentication
Users must register and log in using JWT (JSON Web Tokens) for secure access. The token is stored in local storage for persistence across sessions.

Device Management
Perform CRUD operations (Create, Read, Update, Delete) on devices stored in the SQL database. Users can toggle device status, change names, and manage values.

Real-Time Device Interaction
Users can toggle the status of devices, update their names, and view their details directly in the interface.

Device Table
All devices in the database are displayed in a sortable table, allowing users to sort by ID (lowest to highest), name (alphabetically), and status (on/off or off/on).

Home Page Overview
The Home page provides an overview of the total number of devices that are on and off, along with a table displaying all devices.

Modbus Communication
Connect to a Modbus TCP server, find devices, and manage device attributes such as address, name, and status.

Prerequisites
Before starting, ensure you have the following installed:

.NET SDK (for running backend)

Blazor (for frontend)

SQL Server (for the database)

Modbus TCP Server (for simulation)

NModbus library for Modbus communication