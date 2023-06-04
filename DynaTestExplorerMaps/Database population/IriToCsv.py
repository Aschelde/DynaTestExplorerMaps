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

values = []

count = 0
with open(rsp_file, 'r') as f:
    for line in f:
        # Skip lines that don't start with 5280
        if not line.startswith('5406'):
            continue
            
        fields = line.split(',')

        # Extract the relevant fields
        survey_id = 0
        value_id = count
        distance_begin = (float(fields[1]))*1000
        distance_end = (float(fields[2]))*1000
        iri_value = float(fields[4])

        values.append([survey_id, value_id, distance_begin, distance_end, iri_value])

        count = count+1

# Write the coordinates to a CSV file
with open(csv_file, 'w', newline='') as f:
    writer = csv.writer(f)
    writer.writerow(['survey_id', 'value_id', 'distance', 'distance_begin', 'distance_end', 'iri_value'])
    writer.writerows(values)