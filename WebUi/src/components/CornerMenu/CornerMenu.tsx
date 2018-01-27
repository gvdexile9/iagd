import * as React from 'react';
import { Button, DropdownButton, MenuItem } from 'react-bootstrap';
import './CornerMenu.css';
import { isEmbedded } from '../../constants';

const Image = require('./paypal.png');

class CornerMenu extends React.Component {
  zoomLevel: number = 1.0;
  zoomIncrement: number = 0.25;

  incrementZoom() {
    this.zoomLevel += this.zoomIncrement;
    if (isEmbedded) {
      document.location.href = `setZoom://${this.zoomLevel}`;
    } else {
      console.log(`IncrementZoom to ${this.zoomLevel}`);
    }
  }

  decrementZoom() {
    this.zoomLevel = Math.max(0.1, this.zoomLevel - this.zoomIncrement);
    if (isEmbedded) {
      document.location.href = `setZoom://${this.zoomLevel}`;
    } else {
      console.log(`DecrementZoom to ${this.zoomLevel}`);
    }
  }

  openUrl(url: string) {
    if (isEmbedded) {
      document.location.href = url;
    } else {
      window.open(url);
    }
  }

  render() {
    return (
      <div className="corner-menu">
        <DropdownButton title="Misc" id="corner-dropdown">
          <MenuItem onClick={() => this.openUrl('http://items.dreamcrash.org/ComponentAssembler?record=d009_relic.dbr')}>
            Recipes
          </MenuItem>

          <MenuItem onClick={() => this.openUrl('http://dev.dreamcrash.org/enchantments/')}>
            Components
          </MenuItem>

          <MenuItem onClick={() => this.openUrl('https://www.nexusmods.com/grimdawn/mods/44/?')}>
            Grim Damage
          </MenuItem>

          <MenuItem onClick={() => this.openUrl('https://discord.gg/PJ87Ewa')}>
            Discord
          </MenuItem>
        </DropdownButton>
        <Button onClick={() => this.decrementZoom()}>-</Button>
        <Button onClick={() => this.incrementZoom()}>+</Button>
        <Button onClick={() => this.openUrl('http://grimdawn.dreamcrash.org/ia/?donate')}>
          <img src={Image} height="16" />
        </Button>
      </div>
    );
  }
}

export default CornerMenu;
