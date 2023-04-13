import csv
import os
import xml.etree.ElementTree as ET
from tkinter import filedialog, Tk

# Create a Tkinter root window to host the file dialogs
root = Tk()
root.withdraw()

# Prompt the user to select the XML folder
xml_folder = filedialog.askdirectory(title='Select XML folder')

# Prompt the user to select the CSV file
csv_file = filedialog.asksaveasfilename(title='Save CSV file as', defaultextension='.csv')

# Create a list to store the data
data = []

survey_id = 0

# Loop through the files in the folder
for filename in os.listdir(xml_folder):
    if filename.endswith('.xml'):
        # Parse the XML file
        tree = ET.parse(os.path.join(xml_folder, filename))
        root = tree.getroot()

        # Find the RoadSectionInfo element
        road_section_info = root.find('RoadSectionInfo')

        # Extract the relevant fields
        section_id_elem = road_section_info.find('SectionID')
        section_id = int(section_id_elem.text)

        distance_begin_elem = road_section_info.find('DistanceBegin_m')
        distance_begin = float(distance_begin_elem.text)

        distance_end_elem = road_section_info.find('DistanceEnd_m')
        distance_end = float(distance_end_elem.text)

        time_begin_elem = road_section_info.find('TimeBegin_s')
        time_begin = float(time_begin_elem.text)

        time_end_elem = road_section_info.find('TimeEnd_s')
        time_end = float(time_end_elem.text)

         # Construct the image path
        image_filename = f'LcmsResult_Image3D_{section_id:06d}.jpg'
        image_path = os.path.join(xml_folder, image_filename).replace('/', '\\')

        data.append([survey_id, section_id, distance_begin, distance_end, time_begin, time_end, image_path])

with open(csv_file, 'w', newline='') as f:
    writer = csv.writer(f)
    writer.writerow(['survey_id', 'section_id', 'distance_begin_m', 'distance_end_m', 'time_begin_s', 'time_end_s', 'image_path'])
    writer.writerows(data)
