import React from 'react'
import fetch from 'isomorphic-fetch';

var NorthwindApp = React.createClass({
    getInitialState() {
        return { customers: [], page:0, resultsPerPage:10 };
    },
    graphQlFetcher(query, variables, operationName) {
        return fetch(window.location.origin + this.props.endpoint, {
                method: 'post',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({query:query, variables:variables, operationName: operationName}),
            }).then(response => response.json());
    },
    search() {
        this.graphQlFetcher(`query CustomerQuery($name: String) {
                customers(contactName: $name) {
                    customerId
                    contactName
                }
            }`, {name: this.state.searchTerm})
            .then(response => this.setState({ customers: response.data.customers }));
    },
    componentDidMount() {
        this.load()
    },
    load() {
        this.graphQlFetcher(`query CustomerQuery($limit: Int, $offset: Int) {
                customers(limit: $limit, offset: $offset) {
                    customerId
                    contactName
                }
            }`, {limit: this.state.resultsPerPage, offset: this.state.page * this.state.resultsPerPage})
            .then(response => this.setState({ customers: response.data.customers }));
    },
    prev() {
        this.state.page = Math.max(0, this.state.page - 1);
        this.load();
    },
    next() {
        // TODO limit the max pages
        this.state.page = this.state.page + 1;
        this.load();
    },
    render() {
                    // <div>Search: <input type="text" value={this.state.searchTerm} /><button onClick={this.search}>Go</button></div>
        return (
            <div>
                <div>
                    <h1>Customers:</h1>
                    <ul>
                        {this.state.customers.map(function(c) {
                            return <li key={c.customerId}>{c.contactName}</li>
                        })}
                    </ul>
                    <div>
                    <button onClick={this.prev} disabled={this.state.page <= 0}>&larr; Prev</button>
                    <button onClick={this.next}>Next &rarr;</button>
                    </div>
                </div>
            </div>);
    }
});

module.exports = NorthwindApp;