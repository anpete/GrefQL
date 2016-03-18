import React from 'react'
import fetch from 'isomorphic-fetch';

var allQueries = `query PagingQuery($limit: Int, $offset: Int) {
                customers(limit: $limit, offset: $offset, orderBy: {field: "contactName"}) {
                    customerId
                    contactName
                    address
                },
                customersCount
            }`;

var NorthwindApp = React.createClass({
    getInitialState() {
        return { customers: [], page:0, resultsPerPage:10, maxPages: 1 };
    },
    graphQl(query, operationName, variables) {
        return fetch(window.location.origin + this.props.endpoint, {
                method: 'post',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({query:query, variables:variables, operationName: operationName}),
            }).then(response => response.json());
    },
    componentDidMount() {
        this.load();
    },
    load() {
        this.graphQl(allQueries, "PagingQuery" ,
            {limit: this.state.resultsPerPage, offset: this.state.page * this.state.resultsPerPage}
            )
            .then(response => {
                let maxPages = Number.isInteger(response.data.customersCount)
                    ? Math.ceil(response.data.customersCount / this.state.resultsPerPage)
                    : 1;

                this.setState(
                { 
                    customers: response.data.customers, 
                    maxPages: maxPages
                })
            });
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
            <div className="container">
                <div className="customers">
                    <h1>Customers</h1>
                    <div>{this.state.totalCustomers}</div>
                    <div className="nav-buttons">
                      <button onClick={this.prev} disabled={this.state.page <= 0}>&larr; Prev</button>
                      <button onClick={this.next} disabled={(this.state.page + 1 ) >= this.state.maxPages}>Next &rarr;</button>
                    </div>
                    <ul className="customer-list">
                        {this.state.customers.map(function(c) {
                            return <li key={c.customerId}>
                                <h2>{c.contactName}</h2>
                                <addr>{c.address}</addr>
                            </li>
                        })}
                    </ul>
                </div>
            </div>);
    }
});

module.exports = NorthwindApp;