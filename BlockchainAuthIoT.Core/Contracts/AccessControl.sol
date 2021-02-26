// SPDX-License-Identifier: MIT
pragma solidity >=0.7.0 <0.8.0;

contract AccessControl {

    // Allow policy (default deny)
    struct Policy {
        string resource; // The name of the remote endpoint or resource
        uint256 startTime; // When this policy becomes valid
        uint256 expiration; // When this policy should be voided (unix epoch)

        mapping (string => bool) boolParams; // e.g. "subresourceAccess" => true
        mapping (string => int) intParams; // e.g. "querySize" => 10
        mapping (string => string) stringParams; // e.g. "environment" => "test"
    }

    struct Proposal {
        bool finalized;
        bool approved;
        string resource;
        uint256 startTime;
        uint256 duration;
        BoolParam[] boolParams;
        IntParam[] intParams;
        StringParam[] stringParams;
    }

    struct BoolParam {
        string name;
        bool value;
    }

    struct IntParam {
        string name;
        int value;
    }

    struct StringParam {
        string name;
        string value;
    }

    bool public initialized = false; // Whether the contract has been initialized
    address public owner; // The admin that created the contract
    address[] public admins; // The admins that can propose policy changes
    address user; // The user that signed this contract
    mapping(uint => Policy) public policies; // The active policies for the current user
    mapping(uint => Proposal) public proposals; // The policies proposed by the user
    uint public policiesCount; // Used to generate incremental policy IDs
    uint public proposalsCount; // Used to generate incremental proposal IDs
    uint public adminsCount; // The number of admins (used for enumeration)

    constructor (address signer) {
        // Set the creator as owner and admin
        owner = msg.sender;
        admins.push(msg.sender);
        adminsCount = 1;

        // Set the user that signed the contract
        user = signer;
    }

    /*
    =========
    MODIFIERS
    =========
    */

    modifier onlyAdmin {
        bool isAdmin = false;
        for (uint i = 0; i < admins.length; i++) {
            if (msg.sender == admins[i]) {
                isAdmin = true;
                break;
            }
        }
        require (isAdmin, "Not an admin");
        _;
    }

    modifier onlyOwner {
        require (msg.sender == owner, "Not the owner");
        _;
    }

    modifier onlyUser {
        require (msg.sender == user, "Not the user");
        _;
    }

    modifier onlyNotInitialized {
        require (!initialized, "Contract already initialized");
        _;
    }

    modifier onlyInitialized {
        require (initialized, "Contract not initialized");
        _;
    }

    /*
    ===================================
    PUBLIC FUNCTIONS (OWNER and ADMINS)
    ===================================
    */

    function addAdmin (address newAdmin) public onlyAdmin {
        admins.push(newAdmin);
        adminsCount++;
        emit AdminAdded(newAdmin, msg.sender);
    }

    function removeAdmin (address admin) public onlyOwner {
        require (admin != owner, "Cannot remove the owner");
        bool isAdmin = false;
        uint index;
        for (uint i = 0; i < admins.length; i++) {
            if (admin == admins[i]) {
                isAdmin = true;
                index = i;
                break;
            }
        }

        require (isAdmin, "Admin not found");

        delete admins[index];
        emit AdminRemoved(admin);
    }

    function initializeContract () public onlyAdmin {
        require (!initialized, "Contract already initialized");
        initialized = true;
    }

    // Only admins are able to add policies, when the contract is not
    // yet initialized
    function createPolicy (string memory resource, uint256 startTime, uint256 expiration)
        public onlyAdmin onlyNotInitialized {
            uint policyId = policiesCount++;
            Policy storage newPolicy = policies[policyId];
            newPolicy.resource = resource;
            newPolicy.startTime = startTime;
            newPolicy.expiration = expiration;

            emit PolicyAdded(policyId);
    }

    function setPolicyBoolParam (uint policyId, string memory name, bool value)
        public onlyAdmin onlyNotInitialized {
            Policy storage policy = policies[policyId];
            policy.boolParams[name] = value;
    }

    function setPolicyIntParam (uint policyId, string memory name, int value)
        public onlyAdmin onlyNotInitialized {
            Policy storage policy = policies[policyId];
            policy.intParams[name] = value;
    }

    function setPolicyStringParam (uint policyId, string memory name,
        string memory value) public onlyAdmin onlyNotInitialized {
            Policy storage policy = policies[policyId];
            policy.stringParams[name] = value;
    }

    // Admins can approve proposals
    function approveProposal (uint proposalId) public onlyAdmin onlyInitialized
        returns (uint policyId) {
            Proposal storage proposal = proposals[proposalId];
            require (proposal.finalized, "Proposal not finalized");
            proposal.approved = true;

            // Create a new policy based on that proposal
            policyId = policiesCount++;
            Policy storage policy = policies[policyId];
            policy.resource = proposal.resource;
            // TODO: Fix porting over params
            //policy.boolParams = proposal.boolParams;
            //policy.intParams = proposal.intParams;
            //policy.stringParams = proposal.stringParams;
            policy.startTime = proposal.startTime;
            policy.expiration = proposal.startTime + proposal.duration;

            emit ProposalApproved(proposalId, policyId);
    }

    /*
    =======================
    PUBLIC FUNCTIONS (USER)
    =======================
    */

    // Users are able to create proposals to request access to additional
    // resources (and expand the contract). An admin may then approve the
    // proposal once it's finalized
    function createProposal (string memory resource, uint256 startTime,
        uint256 duration) public onlyUser onlyInitialized
        returns (uint proposalId) {
            proposalId = proposalsCount++;
            Proposal storage newProposal = proposals[proposalId];
            newProposal.approved = false;
            newProposal.finalized = false;
            newProposal.resource = resource;
            newProposal.startTime = startTime;
            newProposal.duration = duration;
    }

    function setProposalBoolParam (uint proposalId, string memory name, bool value)
        public onlyUser onlyInitialized {
            Proposal storage proposal = proposals[proposalId];
            require (!proposal.finalized, "Proposal already finalized");
            BoolParam storage param;
            param.name = name;
            param.value = value;
            proposal.boolParams.push(param);
    }

    function setProposalIntParam (uint proposalId, string memory name, int value)
        public onlyUser onlyInitialized {
            Proposal storage proposal = proposals[proposalId];
            require (!proposal.finalized, "Proposal already finalized");
            IntParam storage param;
            param.name = name;
            param.value = value;
            proposal.intParams.push(param);
    }

    function setProposalStringParam (uint proposalId, string memory name,
        string memory value) public onlyUser onlyInitialized {
            Proposal storage proposal = proposals[proposalId];
            require (!proposal.finalized, "Proposal already finalized");
            StringParam storage param;
            param.name = name;
            param.value = value;
            proposal.stringParams.push(param);
    }

    function finalizeProposal (uint proposalId) public onlyUser onlyInitialized {
        Proposal storage proposal = proposals[proposalId];
        require (!proposal.finalized, "Proposal already finalized");
        proposal.finalized = true;
        emit ProposalFinalized(proposalId);
    }

    /*
    ===========================
    PUBLIC FUNCTIONS (EVERYONE)
    ===========================
    */

    // Get parameter values of proposals
    function getProposalBoolParam (uint proposalId, string memory name) public view
        returns (bool value) {
        Proposal storage proposal = proposals[proposalId];
        for (uint i = 0; i < proposal.boolParams.length; i++) {
            if (compareStrings(proposal.boolParams[i].name, name)) {
                value = proposal.boolParams[i].value;
            }
        }
    }

    function getProposalIntParam (uint proposalId, string memory name) public view
        returns (int value) {
        Proposal storage proposal = proposals[proposalId];
        for (uint i = 0; i < proposal.boolParams.length; i++) {
            if (compareStrings(proposal.boolParams[i].name, name)) {
                value = proposal.intParams[i].value;
            }
        }
    }

    function getProposalStringParam (uint proposalId, string memory name) public view
        returns (string memory value) {
        Proposal storage proposal = proposals[proposalId];
        for (uint i = 0; i < proposal.boolParams.length; i++) {
            if (compareStrings(proposal.boolParams[i].name, name)) {
                value = proposal.stringParams[i].value;
            }
        }
    }

    // Get parameter values of policies
    function getPolicyBoolParam (uint policyId, string memory name) public view
        returns (bool value) {
        Policy storage policy = policies[policyId];
        value = policy.boolParams[name];
    }

    function getPolicyIntParam (uint policyId, string memory name) public view
        returns (int value) {
        Policy storage policy = policies[policyId];
        value = policy.intParams[name];
    }

    function getPolicyStringParam (uint policyId, string memory name) public view
        returns (string memory value) {
        Policy storage policy = policies[policyId];
        value = policy.stringParams[name];
    }

    /*
    =================
    PRIVATE FUNCTIONS
    =================
    */

    // Nothing to see here
    function compareStrings (string memory a, string memory b) internal pure
        returns (bool) {
        return keccak256(abi.encodePacked(a)) == keccak256(abi.encodePacked(b));
    }

    /*
    ======
    EVENTS
    ======
    */

    event AdminAdded (address admin, address sender);
    event AdminRemoved (address admin);
    event PolicyAdded (uint policyId);
    event ProposalFinalized (uint proposalId);
    event ProposalApproved (uint proposalId, uint policyId);
}