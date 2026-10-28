export interface VehicleData {
  id: string;
  name: string;
  category: string;
  tagline: string;
  description: string;
  cardImage: string;
  specs: {
    topSpeedKmh: number;
    acceleration0to100: number; // seconds
    handlingRating: number;     // 1-100
    driftRating: number;        // 1-100
    weightKg: number;
    driveTrain: 'AWD' | 'RWD' | 'FWD';
    boostCapacitySec: number;
  };
  audioPitch: number;
  accentColor: string;
}

export interface RouteData {
  id: string;
  name: string;
  status: 'PLAYABLE' | 'IN_DEVELOPMENT';
  routeNumber: number;
  lengthKm: number;
  laps: number;
  difficulty: 'Easy' | 'Medium' | 'Hard' | 'Extreme';
  surface: string;
  environment: string;
  timeOfDay: string;
  cardImage: string;
  description: string;
  keyFeatures: string[];
  trackRecord: {
    holder: string;
    vehicle: string;
    time: string;
  };
}

export interface LeaderboardEntry {
  rank: number;
  driverName: string;
  vehicle: string;
  category: string;
  routeId: string;
  lapTime: string;
  gap: string;
  platform: 'WebGL Web' | 'Desktop';
  verified: boolean;
  timestamp: string;
}

export const VEHICLES: VehicleData[] = [
  {
    id: 'cheetah-gt',
    name: 'Cheetah GT',
    category: 'Street Tuner',
    tagline: 'Precision Cornering & Coastal Reflexes',
    description: 'Lightweight turbocharged sports coupe engineered for the rapid transitions of downtown alleyways. Optimal downforce balance for street circuit sweepers.',
    cardImage: '/assets/Car_Regular_Card.png',
    specs: {
      topSpeedKmh: 245,
      acceleration0to100: 3.8,
      handlingRating: 88,
      driftRating: 75,
      weightKg: 1280,
      driveTrain: 'AWD',
      boostCapacitySec: 4.5
    },
    audioPitch: 420,
    accentColor: '#00d2ff'
  },
  {
    id: 'apex-hypercar',
    name: 'Apex Hypercar',
    category: 'Exotic Hypercar',
    tagline: 'V10 Twin-Turbo Track Dominator',
    description: 'Uncompromising carbon monocoque and active aerodynamics. Delivers blistering terminal velocity on wide coastal highway straights.',
    cardImage: '/assets/Car_Speedo_Card.png',
    specs: {
      topSpeedKmh: 340,
      acceleration0to100: 2.6,
      handlingRating: 94,
      driftRating: 62,
      weightKg: 1190,
      driveTrain: 'AWD',
      boostCapacitySec: 6.0
    },
    audioPitch: 520,
    accentColor: '#ff2d55'
  },
  {
    id: 'marauder-muscle',
    name: 'Marauder Muscle',
    category: 'Heavy Muscle',
    tagline: '7.0L Supercharged Asphalt Breaker',
    description: 'Raw American-African hybrid displacement. Heavy iron block pushing massive low-end torque for aggressive high-speed slides and barrier exits.',
    cardImage: '/assets/Car_ClassicMuscle_Card.png',
    specs: {
      topSpeedKmh: 265,
      acceleration0to100: 3.5,
      handlingRating: 68,
      driftRating: 89,
      weightKg: 1650,
      driveTrain: 'RWD',
      boostCapacitySec: 5.0
    },
    audioPitch: 290,
    accentColor: '#ffaa00'
  },
  {
    id: 'drift-king',
    name: 'Drift King',
    category: 'Drift Spec',
    tagline: 'Maximum Angle & Hydraulic Precision',
    description: 'Tailored steering rack with 68° lock angle, dual rear calipers, and sequential gearbox. Built to link sweeping metropolitan hairpins effortlessly.',
    cardImage: '/assets/Car_TunerDrift_Card.png',
    specs: {
      topSpeedKmh: 250,
      acceleration0to100: 3.7,
      handlingRating: 82,
      driftRating: 98,
      weightKg: 1220,
      driveTrain: 'RWD',
      boostCapacitySec: 4.8
    },
    audioPitch: 460,
    accentColor: '#00ff88'
  },
  {
    id: 'dune-raider',
    name: 'Dune Raider',
    category: 'Trophy Off-Roader',
    tagline: 'Long-Travel Suspension & Coastal Grip',
    description: 'Reinforced tubular chassis with 18-inch wheel travel and beadlock tires. Completely impervious to roadside curbs, cobblestones, and coastal sand drifts.',
    cardImage: '/assets/Car_StreetRally_Card.png',
    specs: {
      topSpeedKmh: 215,
      acceleration0to100: 4.2,
      handlingRating: 74,
      driftRating: 80,
      weightKg: 1820,
      driveTrain: 'AWD',
      boostCapacitySec: 5.5
    },
    audioPitch: 310,
    accentColor: '#f77f00'
  }
];

export const CIRCUITS: RouteData[] = [
  {
    id: 'downtown-circuit',
    name: 'Downtown Circuit',
    status: 'PLAYABLE',
    routeNumber: 1,
    lengthKm: 4.2,
    laps: 3,
    difficulty: 'Medium',
    surface: 'High-Grip Asphalt with Rain Reflections',
    environment: 'African Coastal Metropolis at Midnight',
    timeOfDay: '00:45 AM (Night Race)',
    cardImage: '/assets/Route_DowntownCircuit.jpg',
    description: 'The premier championship circuit winding through neon-lit Gate A, high-rise luxury towers, and high-speed highway underpasses with dynamic lighting.',
    keyFeatures: [
      '14 High-Speed Checkpoints with Split Timing',
      'Dual Directional AI Opponents with Dynamic Avoidance',
      'Illuminated Billboard Straightaway and Tight Chicane',
      'Instant Finish Line Lap Counter and Results Screen'
    ],
    trackRecord: {
      holder: 'Kofi "Apex" Mensah',
      vehicle: 'Apex Hypercar',
      time: '1:18.42'
    }
  },
  {
    id: 'waterfront-run',
    name: 'Waterfront Run',
    status: 'IN_DEVELOPMENT',
    routeNumber: 2,
    lengthKm: 5.8,
    laps: 2,
    difficulty: 'Hard',
    surface: 'Coastal Boardwalk & Pier Asphalt',
    environment: 'Ocean Boulevard & Palm Promenade',
    timeOfDay: '02:15 AM (Ocean Mist)',
    cardImage: '/assets/Route_WaterfrontRun.jpg',
    description: 'High-speed oceanfront sweepers with sea breeze, luxury yacht harbors, and sweeping high-camber curves facing the midnight tide.',
    keyFeatures: [
      'Extended 1.4km Ocean Straightaway',
      'Wooden Boardwalk Low-Traction Transition',
      'Bioluminescent Wave Reflections along Retaining Walls',
      'Under Development (Asset Production Phase)'
    ],
    trackRecord: {
      holder: 'Target Time',
      vehicle: 'Cheetah GT',
      time: '1:42.10'
    }
  },
  {
    id: 'market-district',
    name: 'Market District',
    status: 'IN_DEVELOPMENT',
    routeNumber: 3,
    lengthKm: 3.4,
    laps: 4,
    difficulty: 'Extreme',
    surface: 'Historic Cobblestone & Urban Asphalt',
    environment: 'Vibrant Night Bazaar & Heritage Arches',
    timeOfDay: '11:30 PM (Festival Lights)',
    cardImage: '/assets/Route_MarketDistrict.jpg',
    description: 'Ultra-technical 90-degree street grids through colorful market stalls, illuminated fabric canopies, and narrow heritage alleys.',
    keyFeatures: [
      'Unforgiving 90-Degree Hairpins with Zero Runoff',
      'Cobblestone Traction Differential requiring Drift Control',
      'Overhanging Lantern Canopies and Street Vendor Booths',
      'Under Development (Geometry Layout Phase)'
    ],
    trackRecord: {
      holder: 'Target Time',
      vehicle: 'Drift King',
      time: '1:09.85'
    }
  },
  {
    id: 'industrial-port',
    name: 'Industrial Port',
    status: 'IN_DEVELOPMENT',
    routeNumber: 4,
    lengthKm: 6.1,
    laps: 2,
    difficulty: 'Hard',
    surface: 'Concrete Slab & Steel Grating',
    environment: 'Shipping Container Terminals & Gantry Cranes',
    timeOfDay: '03:30 AM (Heavy Fog)',
    cardImage: '/assets/Route_IndustrialPort.jpg',
    description: 'Heavy industrial sprawl featuring multi-tiered container chicanes, railway crossings, and expansive harbor crane bridges.',
    keyFeatures: [
      'Container Maze Tight Chicanes and Blind Corner Exits',
      'Suspension-Testing Railway Crossing Jumps',
      'Gantry Crane Illumination and Cargo Loading Platforms',
      'Under Development (Concept & Blockout Phase)'
    ],
    trackRecord: {
      holder: 'Target Time',
      vehicle: 'Marauder Muscle',
      time: '1:54.60'
    }
  }
];

export const INITIAL_LEADERBOARD: LeaderboardEntry[] = [
  {
    rank: 1,
    driverName: 'KofiMensah_KE',
    vehicle: 'Apex Hypercar',
    category: 'Exotic Hypercar',
    routeId: 'downtown-circuit',
    lapTime: '1:18.42',
    gap: 'LEADER',
    platform: 'WebGL Web',
    verified: true,
    timestamp: '2026-10-28 21:14'
  },
  {
    rank: 2,
    driverName: 'Zuberi_Drift',
    vehicle: 'Drift King',
    category: 'Drift Spec',
    routeId: 'downtown-circuit',
    lapTime: '1:19.88',
    gap: '+1.46s',
    platform: 'WebGL Web',
    verified: true,
    timestamp: '2026-10-28 22:40'
  },
  {
    rank: 3,
    driverName: 'Nia_Speedrunner',
    vehicle: 'Cheetah GT',
    category: 'Street Tuner',
    routeId: 'downtown-circuit',
    lapTime: '1:20.31',
    gap: '+1.89s',
    platform: 'WebGL Web',
    verified: true,
    timestamp: '2026-10-28 23:02'
  },
  {
    rank: 4,
    driverName: 'TariqV8_Racer',
    vehicle: 'Marauder Muscle',
    category: 'Heavy Muscle',
    routeId: 'downtown-circuit',
    lapTime: '1:21.05',
    gap: '+2.63s',
    platform: 'Desktop',
    verified: true,
    timestamp: '2026-10-28 19:45'
  },
  {
    rank: 5,
    driverName: 'Amina_Coastal',
    vehicle: 'Cheetah GT',
    category: 'Street Tuner',
    routeId: 'downtown-circuit',
    lapTime: '1:22.18',
    gap: '+3.76s',
    platform: 'WebGL Web',
    verified: false,
    timestamp: '2026-10-28 23:30'
  },
  {
    rank: 6,
    driverName: 'Balo_Rally',
    vehicle: 'Dune Raider',
    category: 'Trophy Off-Roader',
    routeId: 'downtown-circuit',
    lapTime: '1:24.90',
    gap: '+6.48s',
    platform: 'WebGL Web',
    verified: true,
    timestamp: '2026-10-29 00:15'
  }
];
