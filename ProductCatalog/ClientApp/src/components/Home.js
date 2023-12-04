import React, { Component } from 'react';
import ProductDisplay from "./ProductDisplay";
import Sidebar from "./Sidebar";
import {Button} from "reactstrap";

export class Home extends Component {
  static displayName = Home.name;    

    constructor(props) {
        super(props);
        this.state = {products:[], loading: true, filter: {}}
    }
    
    componentDidMount() {
        this.populateProducts();
    }  
    
    static getRows(products){
        const rows = [];
        for(let i =0; i < products.length; i+=5){
            rows.push(products.slice(i, i+5));
        }
        
        return rows;
    }

    queryByImage = async (url) =>{
        const currentFilter = this.state.filter;
        if(currentFilter.description){
            delete currentFilter.description;
        }

        currentFilter.imageUrl = url;         
        await this.filter(currentFilter);
    }

    queryByDescription = async (description) =>{
        const currentFilter = this.state.filter;
        if(currentFilter.imageUrl){
            delete currentFilter.imageUrl;
        }
        
        currentFilter.description = description;
        await this.filter(currentFilter);
    }
    
    updateFilterField = async (fieldName, items) => {
        const currentFilter = this.state.filter;
        currentFilter[fieldName] = items;
        await this.filter(currentFilter);
    }    
    
    nextPage = async ()=>{
        let newOffset = this.state.filter.offset ? this.state.filter.offset : 0;
        newOffset = newOffset + 15;
        await this.updateFilterField("offset", newOffset)
    }
    
    prevPage = async() =>{
        let newOffset = this.state.filter.offset ? this.state.filter.offset : 0;
        newOffset = newOffset - 15;
        await this.updateFilterField("offset", newOffset)
    }
    
    firstPage = async () =>{        
        await this.updateFilterField("offset", 0)
    }
    
    filter = async(filter) =>{        
        const response = await fetch('product/filter',
            {
                method: 'POST',
                headers:{
                    'Content-Type' : 'application/json'
                },
                body: JSON.stringify(filter)
            }
        )
        
        const data = await response.json();
        
        this.setState({products:data, loading:false, filter: filter});        
    }
    
    renderProductsTable(products, facets){
        const rows = Home.getRows(products)        
        return(
            <div>
                <div className="btn-group" style={{display: "flex", justifyContent:"center", width: "100%", alignContent: "center"}}>
                    {(this.state.filter.offset && this.state.filter.offset > 0 && (
                        <div style={{textAlign: "left", paddingTop: "10px", paddingBottom: "10px", paddingRight: "10px"}}>
                            <Button style={{width:"100%"}} className="btn-group-sm" onClick={this.prevPage}>
                                &lt; Prev Page
                            </Button>
                        </div>
                    )) || (<></>)}

                    {(this.state.filter.offset && this.state.filter.offset > 0 && (
                        <div style={{textAlign: "left", paddingTop: "10px", paddingBottom: "10px", paddingRight: "5px" , paddingLeft: "5px"}}>
                            <Button style={{width:"100%"}} className="btn-group-sm" onClick={this.firstPage}>
                                Back to First Page
                            </Button>
                        </div>
                    )) || (<></>)}
                    
                    
                    <div style={{textAlign: "left", paddingTop: "10px", paddingBottom: "10px",paddingLeft: "10px"}}>
                        <Button style={{width:"100%"}} className="btn-group-sm" onClick={this.nextPage}>
                            Next Page &gt;
                        </Button>
                    </div>
                </div>
                <div style={{display: "flex"}}>
                    
                    <Sidebar updateFilter={this.updateFilterField} filterOptions={facets}/>
                    <div>
                        {rows.map((row, rowIndex) => (
                            <div key={rowIndex} className="row">
                                {row.map((product, cellIndex)=>(
                                    <div className="col-md-2" key={cellIndex} style={{width: '20%'}}>
                                        <ProductDisplay product={product} queryByImage={this.queryByImage} queryByDescription={this.queryByDescription}/>
                                    </div>
                                ))}
                            </div>
                        ))}
                    </div>
                </div>                
            </div>
                        
        );
    }

    render() {
        let contents = this.state.loading 
            ? <p><em>loading...</em></p>
            : this.renderProductsTable(this.state.products, this.state.facets)
    return (
        <div>
            <h1 id="tableLabel">Products</h1>            
            {contents}
        </div>
    );
  }  
  
  async populateProducts(){
      const response = await fetch('product?offset=0&limit=15');
      const facets = await fetch('facetoptions');
      const data = await response.json();
      const facetData = await facets.json();
      this.setState({products:data, loading: false, facets:facetData});
  }
}
