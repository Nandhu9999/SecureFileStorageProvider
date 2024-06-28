# SecureFileStorageProvider

## Steps to setup
1. Connect to your database using any sql workbench
1. Create your tables
1. Setup webapi with standard routes
1. Execute scaffold command to
    - Connect to the database using db credentials
	- Convert the tables to relevant models for type inference
	- Automatically will setup ApplicationDbContext.cs
	- Modifiy the ApplicationDbContext.cs so the connection string is coming from the appsettings.json
	- Use your models throughout the webapi


Database:

	- Created on https://aiven.io

	- Host: mysql-xxx.j.aivencloud.com

	- Port: 17125

	- Database: defaultdb

	- Username: avnadmin

	- Password: ********

Scaffold Command:
```
Scaffold-DbContext "Server=mysql-xxx.j.aivencloud.com;Port=17125;Database=defaultdb;User=avnadmin;Password=xxx;" Pomelo.EntityFrameworkCore.MySql -OutputDir Models -Context ApplicationDbContext -Force
```

Packages:

- Pomelo.EntityFrameworkCore.MySql V6.0.3
