import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

csv_files = [
    './performance_metrics_1.csv',
    './performance_metrics_5.csv',
    './performance_metrics_10.csv'
]

data_frames = [pd.read_csv(file) for file in csv_files]
combined_data = pd.concat(data_frames)

x = combined_data['FrameTime_ms']
y = combined_data['Cubes']


highest = combined_data.groupby('Cubes')['FrameTime_ms'].max()
average = combined_data.groupby('Cubes')['FrameTime_ms'].mean()

plt.figure(figsize=(12, 8))
plt.bar(highest.index, highest.values, width=4, label='Highest', align='center')
plt.bar(average.index, average.values, width=4, label='Average', align='center')

plt.xlabel('Number of cubes')
plt.ylabel('FrameTime (in ms)')
plt.title('Highest and Average FrameTime (in ms) for different Number of Cubes')
plt.legend()
plt.grid(True)

plt.xticks(highest.index, highest.index)

ticks = np.concatenate([
    np.arange(0, 150, step=20),
    np.arange(200, highest.max() + 1, step=50)
])
plt.yticks(ticks)

plt.show()