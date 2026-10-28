using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThegameioRacing.Vehicle
{
    [CreateAssetMenu(fileName = "VehicleDatabase", menuName = "THEGAMEIO Racing/Vehicle Database", order = 2)]
    public class VehicleDatabase : ScriptableObject
    {
        [Header("Canonical Vehicle List")]
        [SerializeField] private VehicleConfig[] vehicles = new VehicleConfig[5];

        public IReadOnlyList<VehicleConfig> Vehicles => vehicles;

        public VehicleConfig GetVehicleById(string vehicleId)
        {
            if (vehicles == null) return null;
            for (int i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i] != null && string.Equals(vehicles[i].vehicleId, vehicleId, StringComparison.OrdinalIgnoreCase))
                {
                    return vehicles[i];
                }
            }
            return null;
        }

        public VehicleConfig GetDefaultVehicle()
        {
            if (vehicles != null && vehicles.Length > 0 && vehicles[0] != null)
            {
                return vehicles[0];
            }
            return null;
        }

        public void SetVehicles(VehicleConfig[] list)
        {
            vehicles = list;
        }
    }
}
