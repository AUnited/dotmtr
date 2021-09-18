dotMTR Live
=========================
<hr>
##ABOUT
- dotMTR Live is a modern, graphically enhanced MTR implementation created for Windows.

- dotMTR Live is written in C# for version 4.8 of the .NET Framework

- dotMTR Live is fork of software &copy;  2010 Nate McKay (natemckay@gmail.com)

- dotMTR Live is free software, licensed under the GNU General Public License, version 2 (http://www.gnu.org/licenses/gpl-2.0.txt)

## RELEASE INFORMATION
Version 0.8.0 (released October 20, 2010)

## GENERAL INFORMATION
dotMTR can be called with the Windows /TraceDest= switch to populate a destination (i.e. dotMTR.exe /TraceDest=1.1.1.1)
Tracing is multithreaded and performed asynchronously
One packet per hop is sent for each trace up to the max TTL regardless of distance to target

## KNOWN ISSUES
Memory usage is probably somewhat high for such an application

Graph scaling could be improved in some circumstances

There is currently no way to set or save preferences (though some things can be changed via dotMTR.exe.confg)

## UPCOMING MILESTONE GOALS
- Ability to perform TCP traceroutes (maybe some layer 7 stuff too...)
- Bring memory utilization down a bit
- Provide visual representation of packet loss
- Options & preferences
- Streamline classes
- Make better use of events for statistics updates etc.
- Better align graph edge w/ most recent point updates
- Swap trace panels?
- Proper cleanup of graph?
- Bug fixes (patches are greatly appreciated)

## CREDITS & THANKS
Grid display leverages SourceGrid (http://sourcegrid.codeplex.com/ http://bitbucket.org/dariusdamalakas/sourcegrid/)

Graph display leverages ZedGraph (http://zedgraph.org/wiki/index.php?title=Main_Page)

Thread management leverages SmartThreadPool (http://smartthreadpool.codeplex.com/ http://www.codeproject.com/KB/threads/smartthreadpool.aspx)

Command line processing leverages TestAPI (http://testapi.codeplex.com/)

Progress indicator leverages Circular Progress Indicator (http://www.codeproject.com/KB/progress/ProgressIndicator.aspx)

Message passing implemented with queueing code by William Stacey, MVP (staceyw@mvps.org)

Icon adapted from kfouleggs.png included in the Crystal Project icon theme set (http://everaldo.com/crystal/)

Inspiration courtesty of the hackers behind the original MTR (http://www.bitwizard.nl/mtr/)

Thanks go to the various sites on the 'net hosting free code, code examples, and associated discussion

## NOTES
All libraries are copyright their respective authors and are redistributed without modification (except for ZedGraph)

The included ZedGraph library was compiled from source with one patch to fix tooltip flickering (http://sourceforge.net/tracker/?func=detail&aid=3061209&group_id=114675&atid=669144)
