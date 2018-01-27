import * as React from 'react';
import * as Guid from 'guid';

interface TreeDataNode {
  name: string;
  bitmap: string;
  record: string;
  cost: TreeDataNode[];

  numCraftable: number;
  numOwned: number;
  numRequired: number;
  isComplete: boolean;
}

interface JsTreeNode {
  label: string;
  value: string;
  children: JsTreeNode[];
  icon: JSX.Element;
  checked: boolean;
}

interface Tree {
  rootNode: JsTreeNode;
  checked: string[];
  expanded: string[];
}

function gatherChecked(list: string[], node: JsTreeNode, parentChecked: boolean) {
  if (node.checked || parentChecked) {
    list.push(node.value);
  }

  for (let i = 0; i < node.children.length; i++) {
    gatherChecked(list, node.children[i], node.checked || parentChecked);
  }
}

function gatherExpanded(list: string[], node: JsTreeNode) {
  const isExpanded = !node.checked;

  if (isExpanded) {
    list.push(node.value);

    for (let i = 0; i < node.children.length; i++) {
      gatherExpanded(list, node.children[i]);
    }
  }
}

function parseNode(node: TreeDataNode): JsTreeNode {
  return {
    label: `${node.numOwned}/${node.numRequired} ${node.name}`,
    icon: <img src={node.bitmap} />,
    children: node.cost.map(parseNode),
    checked: node.numOwned >= node.numRequired,
    value: Guid.raw() // TODO: Make this a representation of where it is in the tree instead?
  };
}

function ParseTree(node: TreeDataNode): Tree {
  const rootNode = parseNode(node);

  let checked = [];
  gatherChecked(checked, rootNode, false);

  let expanded = [];
  gatherExpanded(expanded, rootNode);

  return {
    rootNode: rootNode,
    checked: checked,
    expanded: expanded
  };
}

export default ParseTree;
