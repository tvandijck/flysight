using System;
using Brejc.GpsLibrary.Gpx;
using GMap.NET;
using Brejc.GpsLibrary.Gpx.Filtering;

namespace FlySightViewer
{
    delegate void AddEntryCallback(string aName, LogEntry aEntry);

    static class GpxImporter
    {
        public static void Import(string aKey, string aPath, AddEntryCallback aCallback)
        {
            GpxFile file = GpxFile.Load(aPath);
            if (file != null)
            {
                Import(aKey, file, aCallback);
            }
        }

        public static void Import(string aKey, GpxFile aFile, AddEntryCallback aCallback)
        {
            if (aFile != null)
            {
                // apply some basic filtering of loaded GPX data
                FileFilterChain filterChain = new FileFilterChain();
                filterChain.Filters.Add(new RemoveErrorPointsFilter());
                aFile = filterChain.ApplyFilters(aFile);

                // convert to LogEntry.
                int idx = 0;
                foreach (Track track in aFile.Tracks)
                {
                    if (track.Segments.Count > 0)
                    {
                        TrackSegment firstSeg = track.Segments[0];

                        string key = string.Format("{0}/{1}[{2}]", aKey, track.Name, idx);

                        DateTime time = (DateTime)firstSeg.StartTime;
                        LogEntry entry = new LogEntry(key, time, firstSeg.PointsCount);

                        foreach (TrackSegment seg in track.Segments)
                        {
                            foreach (TrackPoint pnt in seg.Points)
                            {
                                Record rec = new Record();
                                rec.Location = new PointLatLng(pnt.Location.Y, pnt.Location.X);

                                if (pnt.Time != null)
                                {
                                    rec.Time = (DateTime)pnt.Time;
                                }

                                if (pnt.Elevation != null)
                                {
                                    rec.Altitude = (float)pnt.Elevation;
                                }

                                entry.Records.Add(rec);
                            }
                        }

                        CalculateMissingData(entry);
                        aCallback(key, entry);
                        idx++;
                    }
                }
            }
        }

        public static void CalculateMissingData(LogEntry aEntry)
        {
            int num = aEntry.Records.Count;
            for (int i = 1; i < num; i++)
            {
                Record a = aEntry.Records[i];
                Record b = aEntry.Records[i - 1];

                // calculate differences.
                double timeDiff = (a.Time - b.Time).TotalSeconds;
                double altitudeDiff = a.Altitude - b.Altitude;
                double latDiff = DistanceInMeters(a.Location.Lat, b.Location.Lat);
                double lngDiff = DistanceInMeters(a.Location.Lng, b.Location.Lng);

                // calculate velocities.
                a.VelocityDown = (float)(-altitudeDiff / timeDiff);
                a.VelocityNorth = (float)(latDiff / timeDiff);
                a.VelocityEast = (float)(-lngDiff / timeDiff);

                // store result.
                aEntry.Records[i] = a;
            }
        }

        public static double DistanceInMeters(double aLat1, double aLat2)
        {
            double R = 6371000.0f; // radius of earth in meters.
            double dLat = DegToRad(aLat2 - aLat1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c;
            return (dLat > 0) ? d : -d;
        }

        public static double DegToRad(double aAngle)
        {
            return (aAngle * Math.PI) / 180.0f;
        }
    }
}
