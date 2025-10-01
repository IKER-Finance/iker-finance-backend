# IKER Finance - Backend API
The server-side application that powers IKER Finance. This handles all the data storage, user accounts, security, and business logic for the personal finance management system.

## Live Demo & Access

### Hosted Environment
- **Live API Documentation**: https://iker-finance.onrender.com/swagger
- **API Base URL**: https://iker-finance.onrender.com/api/v1/

**Note**: Deployed on Render's free tier for demonstration and testing purposes.

### Default Test Account
```
Email: admin@ikerfinance.com
Password: Admin@123456
```

## What This Backend Does
This is the "behind-the-scenes" part of IKER Finance that:

- **Stores Your Data**: Keeps your transactions, budgets, and account information safely in a database
- **Handles User Accounts**: Manages registration, login, and keeps your data secure
- **Processes Requests**: When the frontend asks for data, this backend provides it
- **Manages Security**: Ensures only you can access your financial information
- **Currency Conversion**: Handles automatic currency conversion for your transactions

**Think of it like this**: The frontend (what you see in your browser) is like a restaurant's dining room, and this backend is like the kitchen - it does all the work behind the scenes.

## Before You Start

### What You Need Installed

1. **.NET 8 SDK** - This is Microsoft's platform for running the backend
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Choose "SDK" (not just Runtime)
   - This includes everything needed to run .NET applications

2. **PostgreSQL 17 Database** - This stores all your financial data
   - We'll help you install this in the setup steps below
   - Don't worry if you've never used a database before!

3. **Git** - For downloading the code
   - Download from: https://git-scm.com/
   - Follow installation with default settings

4. **Code Editor** (optional but helpful)
   - Visual Studio Code: https://code.visualstudio.com/
   - Makes it easier to view configuration files

### IMPORTANT: This Works With the Frontend

**This backend must run together with the IKER Finance Frontend.** 
- The backend runs on `http://localhost:5008` (or 5000)
- The frontend connects to this backend to get and save data
- You need both running at the same time for the app to work

## Step-by-Step Setup Guide

### Step 1: Download the Code

Open your terminal or command prompt:

```bash
# Download the project code
git clone https://github.com/IKER-Finance/iker-finance-backend.git

# Go into the project folder
cd iker-finance-backend
```

**Don't have Git?** You can download the code as a ZIP file from GitHub and extract it.

### Step 2: Check Your .NET Installation

```bash
# Verify .NET is installed correctly
dotnet --version
```

You should see something like `8.0.x`. If you get an error, .NET isn't installed properly.

### Step 3: Install Project Dependencies

```bash
# Download all the packages this project needs
dotnet restore

# Build the project to make sure everything works
dotnet build
```

**What's happening?** This downloads all the libraries and compiles the code to check for errors.

### Step 4: Set Up the Database

The application needs a database to store your financial data. We'll use PostgreSQL.

#### For Mac Users

**Easy Option - Using Postgres.app:**
1. Go to https://postgresapp.com/
2. **Important**: The main download might be PostgreSQL 18 - look for PostgreSQL 17 instead
3. Download PostgreSQL 17 version of Postgres.app
4. Install and open the app
5. Click "Initialize" to create the default database server
6. You'll see a green light - that means PostgreSQL 17 is running!

**Alternative - Using Homebrew (if you have it):**
```bash
# Install PostgreSQL 17 specifically
brew install postgresql@17
brew services start postgresql@17
```

**Note**: If you can't find PostgreSQL 17 easily, search for "Postgres.app PostgreSQL 17" or "PostgreSQL 17 Mac download" in your browser.

#### For Windows Users

**Easy Option - PostgreSQL 17 Installer:**
1. Go to https://www.postgresql.org/download/windows/
2. Look for the "PostgreSQL 17" section (not the latest version 18)
3. Click "Download" for PostgreSQL 17
4. Run the downloaded installer
5. Follow the setup wizard with default settings
6. **Important**: Remember the password you create for the "postgres" user
7. Keep the default port (5432)

**Note**: If you can't find PostgreSQL 17 easily, you can also search for "PostgreSQL 17 windows installer download" in your browser.

#### Create Your Database

After PostgreSQL is installed and running, you'll need to create the database. We'll use pgAdmin4, which provides a visual interface (much easier than typing commands).

#### Install pgAdmin4

**For Both Mac and Windows:**
1. Go to https://www.pgadmin.org/download/
2. Download pgAdmin4 for your operating system
3. Install it with the default settings

#### Create the Database Using pgAdmin4

1. **Open pgAdmin4** after installation
2. **First time setup**: It will ask you to set a master password - remember this!
3. **Connect to your PostgreSQL server**:
   - Look for "PostgreSQL 17" in the left sidebar under "Servers"
   - If you don't see it, right-click "Servers" → "Register" → "Server"
   - **Connection tab**: 
     - Host: `localhost`
     - Port: `5432`
     - Username: `postgres`
     - Password: (the password you set when installing PostgreSQL)

4. **Create the database**:
   - Right-click on "Databases" in the left sidebar
   - Select "Create" → "Database"
   - **Database name**: `IkerFinanceDB` (exactly as written)
   - Click "Save"

5. **Verify it worked**:
   - You should see "IkerFinanceDB" appear in the list under "Databases"
   - Success! Your database is ready.

**Alternative: Command Line Method (if pgAdmin doesn't work)**

If pgAdmin4 isn't working for you, try these commands in terminal/command prompt:

**For Mac (using Postgres.app):**
```bash
psql postgres
CREATE DATABASE "IkerFinanceDB";
\q
```

**For Windows:**
```cmd
psql -U postgres
CREATE DATABASE "IkerFinanceDB";
\q
```

### Step 5: Configure the Application

The application needs to know how to connect to your database and other settings.

Navigate to the API folder and create a configuration file:

```bash
# Go to the API project folder
cd src/IkerFinance.API
```

Create a new file called `appsettings.Development.json` (use a text editor) with this content:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=IkerFinanceDB;Username=postgres;Password=your_password_here;Port=5432"
  },
  "JwtSettings": {
    "SecretKey": "your-very-long-secret-key-for-security-at-least-64-characters-long-please",
    "Issuer": "IkerFinance",
    "Audience": "IkerFinance-Users"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Important Changes to Make:**
1. **Replace `your_password_here`** with the PostgreSQL password you created
2. **If you're on Mac using Postgres.app**, you might not have set a password, so just remove the `Password=your_password_here;` part
3. The SecretKey can stay as-is for development

### Step 6: Set Up Database Tables

The application needs to create tables in your database to store data:

```bash
# Install the database tool (if not already installed)
dotnet tool install --global dotnet-ef

# Create the database structure
dotnet ef database update --project ../IkerFinance.Infrastructure --startup-project .
```

**What's happening?** This creates all the tables needed to store users, transactions, budgets, etc.

### Step 7: Start the Backend

```bash
# Start the backend server
dotnet run
```

You should see messages like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5008
      Now listening on: https://localhost:5009
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Step 8: Verify It's Working

1. Open your web browser
2. Go to: `http://localhost:5008/swagger`
3. You should see the API documentation page with a list of endpoints

**Success!** Your backend is now running and ready to work with the frontend.

## Testing the Backend

### Default Admin Account

The system automatically creates a test admin account:
- **Email:** `admin@ikerfinance.com`
- **Password:** `Admin@123456`

You can use this to test login functionality.

### Testing with the Frontend

1. Make sure this backend is running (you should see the "listening on" messages)
2. Start the frontend application
3. Try logging in with the admin account above
4. If login works, both frontend and backend are communicating correctly!

## Common Commands

```bash
# Start the backend (use this every time)
dotnet run

# Stop the backend
# Press Ctrl+C in the terminal

# Rebuild after making changes
dotnet build

# Create database tables (after database changes)
dotnet ef database update --project src/IkerFinance.Infrastructure --startup-project src/IkerFinance.API

# Run tests (to make sure everything works)
dotnet test
```

## Troubleshooting

### Problem: "dotnet: command not found"
**Solution:** .NET SDK isn't installed properly. Re-download and install from Microsoft's website.

### Problem: Database connection errors
**Solution:**
1. **Check PostgreSQL is running:**
   - Mac (Postgres.app): Look for the elephant icon in your menu bar
   - Windows: Check Services or try connecting with `psql -U postgres`
2. **Check your password** in `appsettings.Development.json`
3. **Check the database exists:** Run `psql postgres` and then `\l` to list databases

### Problem: "Port already in use" or similar
**Solution:**
1. Something else is using port 5008
2. Kill the other process or use a different port
3. Or restart your computer to clear all ports

### Problem: Frontend can't connect to backend
**Solution:**
1. **Most common:** Make sure backend is actually running (check for "listening on" messages)
2. **Check the port:** Frontend expects backend on `http://localhost:5008`
3. **Check firewall:** Make sure your firewall isn't blocking the connection
4. **Try the Swagger page:** Go to `http://localhost:5008/swagger` to verify backend is accessible

### Problem: Database migration errors
**Solution:**
1. Make sure PostgreSQL is running
2. Check your connection string in `appsettings.Development.json`
3. Try dropping and recreating the database:
   ```bash
   # Connect to postgres
   psql postgres
   
   # Drop and recreate database
   DROP DATABASE IF EXISTS "IkerFinanceDB";
   CREATE DATABASE "IkerFinanceDB";
   \q
   
   # Run migration again
   dotnet ef database update --project src/IkerFinance.Infrastructure --startup-project src/IkerFinance.API
   ```

### Problem: Build errors
**Solution:**
1. Make sure you're in the right directory (`iker-finance-backend`)
2. Try cleaning and rebuilding:
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

## How to Stop the Backend

- Press `Ctrl+C` in the terminal where it's running
- Close the terminal window
- The backend will stop accepting connections

## Understanding the Project Structure

You don't need to understand this to use the app, but here's what the main folders do:

```
src/
├── IkerFinance.API/              # The web server that handles requests
├── IkerFinance.Application/      # Business logic (what the app actually does)
├── IkerFinance.Domain/           # Core business rules and data models
├── IkerFinance.Infrastructure/   # Database connection and external services
└── IkerFinance.Shared/           # Code shared between different parts

tests/
└── Various test projects         # Code that tests if everything works correctly
```

## What Happens When You Run the Backend

1. **Starts a web server** on your computer (port 5008)
2. **Connects to PostgreSQL** database to store/retrieve data
3. **Creates API endpoints** that the frontend can call
4. **Handles authentication** (login/register requests)
5. **Processes financial data** (transactions, budgets, reports)
6. **Waits for requests** from the frontend application

## Security Notes

**For Development Only:**
- The current setup is for development/testing on your local computer
- Don't use this configuration for a real website
- The admin password should be changed for any real use
- Database passwords should be more secure for production

## Git Workflow and Branching Strategy

If you're contributing to this project or working in a team, here's how we manage code changes using Git.

### What is Git Branching? (Simple Explanation)

Think of Git branches like different versions of the same project:
- **main branch** = The final, working version (like a published book)
- **develop branch** = The draft where we test new features (like a book being edited)
- **feature branches** = Individual chapters being written (like writing one chapter at a time)

### Our Branching Strategy

```
main (production-ready code)
├── develop (development/testing)
    ├── feature/your-feature-name
    ├── feature/another-feature
    └── bugfix/fix-something
```

### Workflow Process

#### 1. Start New Work
- Switch to the `develop` branch
- Create a new branch from `develop` for your feature
- Use clear naming: `feature/what-you-are-doing` or `bugfix/what-you-are-fixing`

**Good branch name examples:**
- `feature/user-profile-page`
- `feature/transaction-filtering`
- `bugfix/login-error-handling`
- `feature/currency-conversion`

#### 2. Do Your Work
- Make your changes to the code
- Test that everything works
- Make sure the backend still starts with `dotnet run`

#### 3. Save and Share Your Changes
- Commit your changes with clear, descriptive messages
- Push your branch to the remote repository

**Good commit message examples:**
- "Add budget management endpoints and validation"
- "Fix login error when password contains special characters"
- "Update user profile page with new fields"

#### 4. Create Pull Request
- Create a pull request from your feature branch to `develop` (not `main`)
- Write a clear title and description of what you changed
- Wait for code review and approval

#### 5. Clean Up
- After your pull request is merged, delete your feature branch
- Switch back to `develop` and pull the latest changes

### Tools You Can Use

You don't need to use command line Git. Many developers use visual Git tools:

- **Fork** (Mac/Windows) - Great visual interface
- **GitHub Desktop** - Simple and free
- **SourceTree** - Feature-rich and free
- **GitKraken** - Beautiful interface with good features
- **VS Code** - Has built-in Git features
- **Command line** - If you prefer typing commands

All these tools can do the same workflow - use whatever you're comfortable with!

### Important Rules to Follow

1. **Never work directly on `main` or `develop` branches**
2. **Always create a feature branch** for your work
3. **Always start feature branches from `develop`** (not from `main`)
4. **Write clear commit messages** that explain what you did
5. **Test your code** before creating a pull request
6. **Keep feature branches small** - one feature at a time
7. **Create pull requests to `develop`** (not to `main`)

### Pull Request Guidelines

When creating a pull request:

**Title**: Clear, concise description
- Good: "Add transaction filtering by date range"
- Bad: "Updates"

**Description**: Explain what and why
- What changes did you make?
- Why did you make these changes?
- How should someone test your changes?

### Why We Use This Workflow

This approach ensures:
- **The main branch is always stable and working**
- **Features are developed in isolation** and don't break other people's work
- **Code is reviewed** before being added to the main project
- **Everyone can work on different features** at the same time without conflicts
- **We have a history** of all changes and can undo things if needed

### First Time Contributing?

If you're new to this workflow:

1. **Pick a small task first** (like fixing a typo or updating documentation)
2. **Practice the workflow** with something simple
3. **Ask questions** if you're unsure about anything
4. **Don't worry about making mistakes** - that's how you learn!

The key is understanding the concept: develop → feature branch → pull request → merge back to develop.

## Getting Help

If you're stuck:

1. **Check the terminal output** for specific error messages
2. **Make sure PostgreSQL is running** (most common issue)
3. **Verify your configuration file** has the correct database password
4. **Try the troubleshooting steps** above for your specific error
5. **Test the Swagger page** at `http://localhost:5008/swagger`
6. **For Git issues**: Ask a team member or search for the specific error message online

## Technologies Used

You don't need to learn these to use the backend, but here's what powers it:

- **.NET 8** - Microsoft's platform for building web applications
- **PostgreSQL** - A powerful, reliable database system
- **Entity Framework** - Makes it easier to work with the database
- **ASP.NET Identity** - Handles user accounts and security
- **JWT Tokens** - Secure way to handle user sessions
- **Clean Architecture** - Organized code structure for maintainability