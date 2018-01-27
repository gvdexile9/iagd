import * as React from 'react';
import { RELIC_TEST_DATA } from '../constants';
import ParseTree from '../logic/TreeParser';
import CheckboxTree from 'react-checkbox-tree';
import 'react-checkbox-tree/lib/react-checkbox-tree.css';

const tree = ParseTree(RELIC_TEST_DATA);
const nodes = [tree.rootNode];

export class Tree extends React.Component {
  state = {
    checked: tree.checked,
    expanded: tree.expanded,
  };

  render() {
    console.log([ParseTree(RELIC_TEST_DATA)]);
    console.log(this.state, tree.checked);

    return (
      <div>
        <br/><br/>
        <CheckboxTree
          nodes={nodes}
          checked={this.state.checked}
          expanded={this.state.expanded}
          onCheck={checked => this.setState({ checked })}
          onExpand={expanded => this.setState({ expanded })}
        />
        <br />
      </div>
    );
  }
}
