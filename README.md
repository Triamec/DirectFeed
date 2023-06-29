# DirectFeed Sample

The DirectFeed sample enables to process arbitrarily big move profiles for one or several axes.
There are many buttons to demonstrate what's happening under the covers. However, the Auto Loop
button as well as a command line switch allow for automating repeated profile moves.
Additionally, when dragging a move profile file over the application icon, it will automatically be executed.
Please read and follow the recommendations in this document before first starting the application.

Please refer to the [Manual](./doc/SWNET-DirectFeedOverview-EP007.pdf) for detailed explanations

## 1 Configuration
This document presumes Visual Studio as IDE to use with the sample projects. When starting Visual
Studio, one or more security warnings will be shown. In order to work properly, choose “Load project
normally” and acknowledge the dialog.
When first opening the solution, set the `DirectFeedApplication` project as startup project.

### 1.1 Hardware Environment
Several configurations within the `DirectFeed.sln` must be adjusted to the actual hardware environment to execute the application for.
The starting point is a working TAM configuration for your system. Refer to the Drive Setup User Guide
on how to set up a configuration.
If you didn’t make the configuration persistent on the drive, you’ll need to set up the path to the configuration by starting this application, then going to preferences using menu **File | Preferences…**, and
setting the *TAM configuration* file preference in the *Startup* category appropriately.

*Note This application works with the default Triamec workspace on the system. That is, changing
preferences as described above may affect other applications as well.*

As next step, configure the preferences shown in the *DirectFeed* category.
At least, the *Axes* preference needs your attention. Initially, there are no axes configured. Clicking on
the ellipsis on the right will show a collection editor which allows you to add, reorder and modify individual axis configurations. The application identifies each axis by the name of the station and the index
of the axis within the drive. *Encoder Type* has to be left as *Analog* for digital and analog encoders, and
set to *Endat* for EnDat digital encoders.

The order of the axes is significant in several aspects:

- The order must be according to the data columns.
- If there are mechanical constraints, the order defines the sequence the axes move to their start and park positions. The application moves the axes to the start positions one by one in the order they are defined, and in reverse order back to park position.
- 
The `PositionDimensionality` preference affects the amount of data sent to one axis. The application supports the following values:

- 1 – send position.
- 2 – send position and velocity.
- 3 – send position, velocity, and acceleration.

The imported data must have an according layout.

## 2 Motion Profiles
A simple motion profile, `data.txt`, is shipped as example, look in the `DirectFeedApplication`
project.
The motion profile needs to correspond with the `Axis` and `PositionDimensionality` preferences
as introduced in chapter 1.1 above. For each specified axis, `PositionDimensionality` number of
columns need to be specified, in correct order. Columns exceeding that requirement will be ignored.
Beneath the provided sample `data.txt` profile, simple profiles may be created using the scope of the
TAM System Explorer as follows:

- Of the path planner signals, plot position, velocity, and acceleration of each axis with 10kHz while moving.
- Save plots as comma separated value file.
- Modify the first two data rows such that the first data row contains the park position and the second data row the start position. The start position is the first actual row of the motion profile.
