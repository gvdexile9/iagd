import * as React from 'react';
import { Button } from 'react-bootstrap';
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

  openPaypal() {
    if (isEmbedded) {
      document.location.href = 'http://grimdawn.dreamcrash.org/ia/?donate';
    } else {
      window.open('http://grimdawn.dreamcrash.org/ia/?donate');
    }
  }

  render() {
    return (
      <div className="corner-menu">
        <Button onClick={() => this.decrementZoom()}>-</Button>
        <Button onClick={() => this.incrementZoom()}>+</Button>
        <Button onClick={() => this.openPaypal()}><img src={Image} height="16" /></Button>
      </div>
    );
  }
}

export default CornerMenu;
