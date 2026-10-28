import React, { useState } from 'react';
import { Navbar } from './components/Navbar';
import { HeroSection } from './components/HeroSection';
import { WebPlayerSection } from './components/WebPlayerSection';
import { GarageShowcase } from './components/GarageShowcase';
import { RouteGuide } from './components/RouteGuide';
import { ControlsGuide } from './components/ControlsGuide';
import { LeaderboardSection } from './components/LeaderboardSection';
import { ArchitectureModal } from './components/ArchitectureModal';
import { ApiDocsModal } from './components/ApiDocsModal';
import { Footer } from './components/Footer';
import { VEHICLES } from './data/gameData';
import type { VehicleData, RouteData } from './data/gameData';

export const App: React.FC = () => {
  const [selectedVehicle, setSelectedVehicle] = useState<VehicleData>(VEHICLES[0]);
  const [isArchitectureOpen, setIsArchitectureOpen] = useState(false);
  const [isApiDocsOpen, setIsApiDocsOpen] = useState(false);

  const scrollToPlay = () => {
    const el = document.getElementById('play');
    if (el) el.scrollIntoView({ behavior: 'smooth' });
  };

  const scrollToGarage = () => {
    const el = document.getElementById('garage');
    if (el) el.scrollIntoView({ behavior: 'smooth' });
  };

  const handleRaceWithVehicle = (v: VehicleData) => {
    setSelectedVehicle(v);
    scrollToPlay();
  };

  const handlePlayRoute = (_route: RouteData) => {
    scrollToPlay();
  };

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      {/* Top Navigation */}
      <Navbar
        onOpenArchitecture={() => setIsArchitectureOpen(true)}
        onPlayClick={scrollToPlay}
      />

      {/* Main Page Flow */}
      <main style={{ flex: 1 }}>
        <HeroSection
          onPlayClick={scrollToPlay}
          onExploreGarage={scrollToGarage}
        />

        <WebPlayerSection
          selectedVehicle={selectedVehicle}
        />

        <GarageShowcase
          selectedVehicle={selectedVehicle}
          onSelectVehicle={setSelectedVehicle}
          onRaceWithVehicle={handleRaceWithVehicle}
        />

        <RouteGuide
          onPlayRoute={handlePlayRoute}
        />

        <ControlsGuide />

        <LeaderboardSection
          onOpenApiDocs={() => setIsApiDocsOpen(true)}
        />
      </main>

      {/* Technical Modals */}
      <ArchitectureModal
        isOpen={isArchitectureOpen}
        onClose={() => setIsArchitectureOpen(false)}
      />

      <ApiDocsModal
        isOpen={isApiDocsOpen}
        onClose={() => setIsApiDocsOpen(false)}
      />

      {/* Footer */}
      <Footer />
    </div>
  );
};

export default App;
