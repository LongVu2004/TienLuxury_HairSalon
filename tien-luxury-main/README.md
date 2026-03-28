<div align="center">

# TienLuxury

### Comprehensive Hair Salon Management Solution

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-green.svg)](https://docs.microsoft.com/dotnet/csharp/)
[![MongoDB](https://img.shields.io/badge/MongoDB-7.0+-brightgreen.svg)](https://www.mongodb.com/)
[![Frontend](https://img.shields.io/badge/Frontend-HTML%2FCSS%2FJS%2FBootstrap-orange.svg)](https://getbootstrap.com/)
[![Architecture](https://img.shields.io/badge/Architecture-MVC-blue.svg)](https://docs.microsoft.com/aspnet/core/mvc/overview)

[Features](#-key-features) • [Tech Stack](#-technologies-used) • [Installation](#-installation)

TienLuxury is a web application built with ASP.NET Core, providing a management and business platform for TienLuxury store.

</div>



## Key Features

- **Product Management**: Display and manage product lists
- **Service Management**: Provide and manage services
- **Shopping Cart**: Online shopping functionality
- **Reservations**: Customer appointment booking system
- **Employee Management**: Manage employee information
- **Order Management**: Track and process orders
- **Contact**: Contact message system

## Technologies Used

- **Framework**: ASP.NET Core (.NET 9.0)
- **Database**: MongoDB
- **Front-end**: 
  - HTML, CSS, JavaScript
  - Bootstrap
- **Architecture**: MVC (Model-View-Controller)

## Project Structure

```
TienLuxury/
├── Areas/                # Admin area
├── Controllers/          # Logic controllers
├── Data/                 # Database configuration
├── Models/               # Data models
├── Services/             # Business logic services
├── ViewModels/           # View models
├── Views/                # Interface views
└── wwwroot/              # Static files (CSS, JS, Images)
```

## System Requirements

- .NET 9.0 SDK
- MongoDB
- Visual Studio 2022 or Visual Studio Code

## Installation

1. Clone repository:
```bash
git clone https://github.com/LongVu2004/TienLuxury_HairSalon.git
```

2. Navigate to project directory:
```bash
cd tien-luxury-main
```

3. Restore packages:
```bash
dotnet restore
```

4. Create `.env` file in the root directory and configure environment variables:

```env
# MongoDB Configuration
MONGODB_URI=your_mongodb_atlas_uri
MONGODB_DATABASE=your_database_name
```

5. Run the application:
```bash
dotnet run
```

## Author

- LongVu2004
- hotrong14
- trongphuoc21

## License


Copyright © 2025 TienLuxury. All rights reserved.
