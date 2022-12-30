using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure;
using Azure.AI.AnomalyDetector;
using Azure.AI.AnomalyDetector.Models;
using static System.Environment;

namespace anomaly_detector_quickstart
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string endpoint = GetEnvironmentVariable("ANOMALY_DETECTOR_ENDPOINT");
			string apiKey = GetEnvironmentVariable("ANOMALY_DETECTOR_API_KEY");

			var endpointUri = new Uri(endpoint);
			var credential = new AzureKeyCredential(apiKey);

			//create client
			AnomalyDetectorClient client = new AnomalyDetectorClient(endpointUri, credential);

			//read data
			//example: string datapath = @"c:\test\request-data.csv";
			string datapath = @"request-data.csv";

			List<TimeSeriesPoint> list = File.ReadAllLines(datapath, Encoding.UTF8)
							.Where(e => e.Trim().Length != 0)
							.Select(e => e.Split(','))
							.Where(e => e.Length == 2)
							.Select(e => new TimeSeriesPoint(float.Parse(e[1])) { Timestamp = DateTime.Parse(e[0]) }).ToList();

			//create request
			DetectRequest request = new DetectRequest(list)
			{
				Granularity = TimeGranularity.Daily
			};

			EntireDetectResponse result = client.DetectEntireSeries(request);

			bool hasAnomaly = false;
			for (int i = 0; i < request.Series.Count; ++i)
			{
				if (result.IsAnomaly[i])
				{
					Console.WriteLine("Anomaly detected at index: {0}.", i);
					hasAnomaly = true;
				}
			}
			if (!hasAnomaly)
			{
				Console.WriteLine("No anomalies detected in the series.");
			}
		}
	}
}