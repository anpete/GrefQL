import React from 'react'
import ReactDOM from 'react-dom';
import NorthwindApp from './NorthwindApp.jsx';

var endpoint = '/graphql';

ReactDOM.render(<NorthwindApp endpoint={endpoint}/>, document.getElementById('app'));