mcxNOW Transaction Log
======================
This Windows program enables you to read the transaction log page from the [mcxNOW] (https://mcxnow.com/) crypto exchange and store it in a MySQL database.
It also provides you with a feature to export the contents of the transaction database to a CSV file for use with e.g. Microsoft Excel.



Donations
=========
Donations are kindly excepted as a token of gratitude and necessary if you would like to see work on this project continuing.

* BTC	- 1HunuNHKpZiyioBWgLtjEDbLbMMhA1sQd2
* LTC	- LfuDrcZ8vgqezeYYyhy3oowvm9ozvRAw1a
* MNC	- MBVDVH2eRjYad4nrrLR4AHmB1wLz1TGE63
* WDC	- WZxEVoQM2tUspW2ECXR9LUfTFiJcSwwPZb
* XPM	- AZWBpH7VCanYfvH9AT9Lcrc5BBStUBrcn8
* PPC	- PDJeBjixqVUjTtndXjA6Lgkzv6K3mLiBhV
* FTC	- 6qBW7BGEGXeQ69ekNGHtJcmVCNFokA5hbB
* CL	- CQitPwQwMSAodrauRA3pcZdVDuVUsHCjsy
* SC	- sZx7ZMVTH2VsW55TkYnfpq9fAfuLZuoi1J

Prerequisites
-------------
This program will only run if you have MySQL running on your machine, or have access to a remote instance of MySQL. To install MySQL locally you
can download it from [here] (http://dev.mysql.com/downloads/mysql/)

Remember the user and password you chose during intstallation, as you will need them when accessing MySQL from the program.


Manual
------
On start-up of the program, a window will open which gives you the following options:


* Login Method
* Database
* Browser selection / Username and password fields


Login method
------------
Here you can select whether you want to log in on mcxNOW using the browser stored cookies, or by using your username and password.
Using the cookies is only possible when you have used the selected browser to successfully log in on mcxNOW before.

When you select "Username + password" the radio buttons for browser selection are changed for input field for the username and password you use to login in mcxNOW.

**Remark: Make sure to change the settings on mcxNOW to never log out, otherwise the program will stop working when you are logged out by mcxNOW!!!!**

Database
--------
In this section you can enter the data for the MySQL database. 
The host field expects the IP address for the MySQL database. If you run MySQL locally the defaultlocalhost can be used.
Username and password should be set to the appropriate values.

Browser selection
-----------------
Select the browser you have used to successfully usewd to login in mcxNOW.

**OR depending on login method choses**

Username and password
---------------------
select the username and password you use for mcxNOW.

Export CSV
----------
Clicking this button will pop up a file dialog where you can enter a filename for the export. The contents of the database wil then be
written to this file in CSV format.If the database is empty the file will abviously be empty as well.

**If the file exists the content will be overwritten.**

Start Bot
---------
Clicking this button will start retrieving the transaction information from mcxNOW and store it in MySQL. It will keep logging as long as
the program runs and cannot be stopped otherwise than closing the program.
It is possible to use the export functionality while the bot is retrieving transaction data.

Questions and requests
----------------------
If you have a question, fature request or bug report, please send me a PM [BitcoinTalk] (https://bitcointalk.org/) nickname MarpleTrading.


Acknowledgements
================
mcxTransactionLog uses an adapted and extended version of the mcxNOW API SDK written by Michal Gebauer.
