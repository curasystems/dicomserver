dicomserver
-----------

This is a simple test program (works with a test database) for
creating a functional dicmom query server. find-scp and move-scp.

The main solution is dicomserver/dicomserver.sln. The testclient
is not finished (or even properly started).

It is based on [rcd/mdcm][1]

  [1]: https://github.com/rcd/mdcm

The program uses SQL Linq to query a database (schema not included)

The code is not particularily clean, since it was intended as
a test to get mdcm work as expected and test its abilities.
Some parts of the code are hardcoded, some are in config and some
are just commented out (like the test code to limit the move
bandwidth for certain clients).

A database is not yet included, so it wont work out of the
box, but it would be trivial to create or rework that part.

It still creates quite a lot of output on the command line for 
debugging purposes.

Hope this helps anybody trying to get mdcm working for this.

If you find any issues or would like to discuss simply send a mail to
*m.goetzke@curasystems.de*


Note on mdcm
------------

We have traditonally used our own custom dicom libraries, but since
we are shifting away from delivering value there and moving into other
areas, we have tried a number of external dicom libs, especially for
networking.
And even though mdcm is a small project, it is more than
sufficient and pretty well implemented. It is a pleasure to work with
and we will likely switch to contribute our own knowledge into that project
in the foreseeable future.