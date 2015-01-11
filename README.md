# Charter
This program is derived from my efforts to create a share charting program, currently there is a lot of spagetti code as that program was the end result of lots of experimentation.
It uses OxyPlot for charts. The program uses WPF, but I found that the WPF version of OxyPlot is much slower than the WinForm one, so I am using a WindowsFormsHost in XAML. Although this makes the charting acceptable the OxyPlot solution to draw annotations is very slooowwww. I subsequenlty implemented an overlay canvas and uses Windows API's to do the drawing.

For now the program is stil non functional.
