from pathlib import Path
import csv
import io

windows_path = Path(r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\AGF-NoEAC-AudioOptionsPlus-v1.0.1\Config\XUi_Menu\windows.xml")
windows_text = windows_path.read_text(encoding="utf-8")

old_weather_line = '<options_combo_custom name="AOPVolumeProfilesWeatherStorm" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesWeatherStorm" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesWeatherStormTooltip"/>'
new_weather_lines = (
    '<options_combo_custom name="AOPVolumeProfilesWeather" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesWeather" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesWeatherTooltip"/>\n'
    '\t\t\t\t\t\t\t<options_combo_custom name="AOPVolumeProfilesThunder" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesThunder" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesThunderTooltip"/>\n'
    '\t\t\t\t\t\t\t<options_combo_custom name="AOPVolumeProfilesWeatherAlert" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesWeatherAlert" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesWeatherAlertTooltip"/>'
)

weather_thunder_block = (
    '<options_combo_custom name="AOPVolumeProfilesWeather" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesWeather" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesWeatherTooltip"/>\n'
    '\t\t\t\t\t\t\t<options_combo_custom name="AOPVolumeProfilesThunder" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesThunder" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesThunderTooltip"/>\n'
    '\t\t\t\t\t\t\t<rect/>'
)

weather_thunder_alert_block = (
    '<options_combo_custom name="AOPVolumeProfilesWeather" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesWeather" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesWeatherTooltip"/>\n'
    '\t\t\t\t\t\t\t<options_combo_custom name="AOPVolumeProfilesThunder" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesThunder" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesThunderTooltip"/>\n'
    '\t\t\t\t\t\t\t<options_combo_custom name="AOPVolumeProfilesWeatherAlert" name_localization_prefix="xuiOptionsAudio" prefname="AOPVolumeProfilesWeatherAlert" type="ComboBoxFloat" value_min="0" value_max="1" value_increment="0.05" format_string="0%" tooltip_key="xuiOptionsAudioAOPVolumeProfilesWeatherAlertTooltip"/>\n'
    '\t\t\t\t\t\t\t<rect/>'
)

if old_weather_line in windows_text:
    windows_text = windows_text.replace(old_weather_line, new_weather_lines, 1)
elif weather_thunder_block in windows_text and "AOPVolumeProfilesWeatherAlert" not in windows_text:
    windows_text = windows_text.replace(weather_thunder_block, weather_thunder_alert_block, 1)
elif "AOPVolumeProfilesWeatherAlert" in windows_text:
    pass
else:
    raise SystemExit("weather control block not found in live windows.xml")

windows_path.write_text(windows_text, encoding="utf-8")

localization_path = Path(r"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\AGF-NoEAC-AudioOptionsPlus-v1.0.1\Config\Localization.csv")
rows = list(csv.reader(localization_path.read_text(encoding="utf-8").splitlines()))
output_rows = []
inserted = False

for row in rows:
    key = row[0] if row else ""

    if key == "xuiOptionsAudioAOPVolumeProfilesWeatherStorm":
        row[0] = "xuiOptionsAudioAOPVolumeProfilesWeather"
        row[6] = "Weather"
        output_rows.append(row)
        continue

    if key == "xuiOptionsAudioAOPVolumeProfilesWeatherStormTooltip":
        row[0] = "xuiOptionsAudioAOPVolumeProfilesWeatherTooltip"
        row[6] = "Adjusts general weather ambience, such as rain, wind, gusts, fog, blizzards, and sandstorms. Does not affect thunder."
        output_rows.append(row)

        thunder_row = list(row)
        thunder_row[0] = "xuiOptionsAudioAOPVolumeProfilesThunder"
        thunder_row[6] = "Thunder"
        output_rows.append(thunder_row)

        thunder_tooltip_row = list(row)
        thunder_tooltip_row[0] = "xuiOptionsAudioAOPVolumeProfilesThunderTooltip"
        thunder_tooltip_row[6] = "Adjusts thunder sounds only."
        output_rows.append(thunder_tooltip_row)

        inserted = True
        continue

    if key == "xuiOptionsAudioAOPVolumeProfilesThunderTooltip":
        output_rows.append(row)

        weather_alert_row = list(row)
        weather_alert_row[0] = "xuiOptionsAudioAOPVolumeProfilesWeatherAlert"
        weather_alert_row[6] = "Weather Alert"
        output_rows.append(weather_alert_row)

        weather_alert_tooltip_row = list(row)
        weather_alert_tooltip_row[0] = "xuiOptionsAudioAOPVolumeProfilesWeatherAlertTooltip"
        weather_alert_tooltip_row[6] = "Adjusts the weather alert sound only."
        output_rows.append(weather_alert_tooltip_row)

        inserted = True
        continue

    output_rows.append(row)

if not inserted and not any((row and row[0] == "xuiOptionsAudioAOPVolumeProfilesWeatherAlert") for row in output_rows):
    raise SystemExit("weather localization rows not found in live Localization.csv")

buffer = io.StringIO()
writer = csv.writer(buffer, lineterminator="\n")
writer.writerows(output_rows)
localization_path.write_text(buffer.getvalue(), encoding="utf-8")

print("Updated live windows.xml and Localization.csv")