AcmUtility
==========

Some utility functions for AutoCAD Mechanical 

One converts all the Construction Lines (AcadXline/Xline/AcDbXline - in case someone is searching for a different name :)) to Line entities (AcadLine/Line/AcDbLine). 
You may need this if you have xline's in a block and you would like to snap to them outside the block. 
It does not seem possible, whereas line's are snappable even outside the block.

The  other overrides the LTSCALE value before using the HATCH command and then sets it back afterwards. 
You may need this if you have dashed lines closing an area you want to hatch - the hatch preview looks OK but the created hatch might extend beyond the selected area. 
This can be prevented by using a smaller LTSCALE value:
