# SecureFileStorageProvider

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

- - Pomelo.EntityFrameworkCore.MySql V6.0.3