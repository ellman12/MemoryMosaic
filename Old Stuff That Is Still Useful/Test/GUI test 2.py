from datetime import datetime
import tkinter as tk
from tkinter import *
from tkinter import ttk
from tkinter import Text
from tkcalendar import Calendar

def getSelectedDate():
    newDate = calWidget.selection_get()
    print(newDate)
    return newDate

calendarWindow = tk.Tk() # Creating window
calendarWindow.title("Enter New Date & Time")
calendarWindow.geometry("200x200")
Label = tk.Label(calendarWindow, text="Enter New Time").pack()
timeEntry = Text(calendarWindow, width=75, height = 75).pack()

top = tk.Toplevel(calendarWindow) # I have no idea what this does.

calWidget = Calendar(top, font="Arial 14", selectmode='day')
for i in range(6): # Remove week numbers column. https://stackoverflow.com/a/50697434
    calWidget._week_nbs[i].destroy()
    
calWidget.pack(fill="both", expand=True)
ttk.Button(top, text="OK", command=getSelectedDate).pack()

calendarWindow.mainloop()