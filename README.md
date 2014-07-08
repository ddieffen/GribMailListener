GribMailListener
================

This program is intended to be run as a command line program. The task it will perform is to read new incoming emails 
from an IMAP server. Each new email is going to be parsed to check if known queries are present, and respond to these 
queries.

The first purpose of this program is to allow to run complex operation (data, or cpu intensive) on a remote computer
using a very low amount of transfered data. Mainly because we are going to send queries and recive answer using a very
low bandwith and high latency connection (Satellite Globalstar or Iridium phone at respectively 9.6kB/sec and 2.4 kB/sec)

Usage
=====

Without modifying anything the program and be ran and need the user input the first time it starts up. The necessary 
information is to setup the IMAP and SMTP parameters. These will be used to read emails and send responses.

As this program needs to be robust against connection losses, losses of power and reboots, the software will not prompt
anything to the user after the first time. That way if the computer running the software has any issue, the software will
restart using the data stored in the user.config file.

Note: Passwords are stored using some encryption, but can be decrypted if one has access to the user account running 
the program.

Queries
=======

Supported queries now are:
- Sending a request to saildocs.com for weather information on the behalf of the sender though the SMTP account
- Sending an email to another recipient on the behalf of the sender though the SMTP account

These two queries are there because we're using a limited satellite email service that only allows us to send emails
to the email used for registration... Therefore we needs these forwarding capabilities to send emails to saildocs or
other people.

This could be extended in order to grab other weather informations if we were finding a way of using the Pearl scripts
provided on the NOAA website to get grib files or subset of grib files.

Other supported features are:
- Requesting race information from the yellowbrick website, as the Javascript applet is still data intensite, there is
no way we can open that page though a satellite connection. Using the email service will allow us access to any races
using very few amount of data
- Requesting leaderboard information from a yellowbrick race and section. This will allow us to grab the teams order
distance to finish, exact position... From a defined section. Again this will be used as our very low bandwith connection
does not allow us to load the original javascrip applet.



