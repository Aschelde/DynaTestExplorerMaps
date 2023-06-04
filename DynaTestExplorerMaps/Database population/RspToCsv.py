import csv
import os
from tkinter import filedialog, Tk

# Create a Tkinter root window to host the file dialogs
root = Tk()
root.withdraw()

# Prompt the user to select the RSP file
rsp_file = filedialog.askopenfilename(title='Select RSP file', filetypes=[('RSP files', '*.rsp')])

# Prompt the user to select the CSV file
csv_file = filedialog.asksaveasfilename(title='Save CSV file as', defaultextension='.csv')

coordinates = []

count = 0
with open(rsp_file, 'r') as f:
    for line in f:
        # Skip lines that don't start with 5280
        if not line.startswith('5280'):
            continue
            
        fields = line.split(',')

        # Extract the relevant fields
        survey_id = 0
        point_id = count
        distance = float(fields[1])
        time = fields[4]
        latitude = float(fields[5])
        longitude = float(fields[6])
        elevation = float(fields[7])

        coordinates.append([survey_id, point_id, distance, time, latitude, longitude, elevation])

        count = count+1

# Write the coordinates to a CSV file
with open(csv_file, 'w', newline='') as f:
    writer = csv.writer(f)
    writer.writerow(['survey_id', 'point_id', 'distance', 'time', 'latitude', 'longitude', 'altitude'])
    writer.writerows(coordinates)