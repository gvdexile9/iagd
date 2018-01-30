import * as React from 'react';
import ParseTree from '../../logic/TreeParser';
import CheckboxTree from 'react-checkbox-tree';
import 'react-checkbox-tree/lib/react-checkbox-tree.css';
import { Component } from './types';

interface Props {
  selectedRecipe: Component;
}

interface InternalState {
  checked: string[];
  expanded: string[];
  nodes: {};
}
export class Tree extends React.Component<Props, {}> {
  state: InternalState;

  constructor(props: Props) {
    super(props);

    const tree = ParseTree(props.selectedRecipe);
    const nodes = [tree.rootNode];
    this.state = {
      checked: tree.checked,
      expanded: tree.expanded,
      nodes: nodes
    };
  }


  componentWillReceiveProps(props: Props) {
    const tree = ParseTree(props.selectedRecipe);
    const nodes = [tree.rootNode];

    this.setState({
      checked: tree.checked,
      expanded: tree.expanded,
      nodes: nodes
    });
  }

  render() {
    return (
      <div>
        <br/><br/>
        {this.props.selectedRecipe.name !== '' &&
        <div>
          <h3>Crafting recipe for {this.props.selectedRecipe.name}</h3>
          <CheckboxTree
            nodes={this.state.nodes}
            checked={this.state.checked}
            expanded={this.state.expanded}
            onCheck={checked => this.setState({ checked })}
            onExpand={expanded => this.setState({ expanded })}
          />
          <h3>You are currently lacking: TODO</h3>
        </div>
        }
        <br />
      </div>
    );
  }
}
